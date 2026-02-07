using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VeilleNet.Models;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.News;

public interface IXPostsService
{
    Task<List<XPost>> GetLatestOfficialPostsAsync();
}

public class XPostsService : IXPostsService
{
    private const string CacheKey = "OfficialXPosts";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cacheService;
    private readonly XApiOptions _options;
    private readonly ILogger<XPostsService> _logger;
    private readonly string _cacheFilePath;

    private readonly List<XAccount> _accounts =
    [
        new("github", "github"),
        new("AnthropicAI", "AnthropicAI"),
        new("OpenAI", "OpenAI"),
        new("GeminiApp", "GeminiApp"),
        new("msdev","Microsoft Developer")
    ];

    public XPostsService(
        IHttpClientFactory httpClientFactory,
        ICacheService cacheService,
        IOptions<XApiOptions> options,
        ILogger<XPostsService> logger,
        IHostEnvironment hostEnvironment)
    {
        _httpClientFactory = httpClientFactory;
        _cacheService = cacheService;
        _options = options.Value;
        _logger = logger;

        var cacheDirectory = Path.Combine(hostEnvironment.ContentRootPath, "cache");
        Directory.CreateDirectory(cacheDirectory);
        _cacheFilePath = Path.Combine(cacheDirectory, "official-x-posts.json");
    }

    public async Task<List<XPost>> GetLatestOfficialPostsAsync()
    {
        var cachedPosts = _cacheService.Get<List<XPost>>(CacheKey);
        if (cachedPosts != null)
        {
            return cachedPosts;
        }

        var diskCachedPosts = await TryGetDiskCacheAsync();
        if (diskCachedPosts != null)
        {
            _cacheService.Set(CacheKey, diskCachedPosts, GetCacheDuration());
            return diskCachedPosts;
        }

        if (string.IsNullOrWhiteSpace(_options.BearerToken))
        {
            _logger.LogWarning("X API bearer token is missing. Skipping official posts fetch.");
            return [];
        }

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_options.BaseUrl, UriKind.Absolute);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.BearerToken);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Containsharp/1.0");

        var posts = new List<XPost>();
        var maxResults = _options.PostsPerUser > 0 ? _options.PostsPerUser : 4;

        foreach (var account in _accounts)
        {
            var user = await GetUserAsync(httpClient, account.Username);
            if (user == null)
            {
                continue;
            }

            var accountPosts = await GetUserPostsAsync(httpClient, user.Id, account, user.ProfileImageUrl, maxResults);
            posts.AddRange(accountPosts);
        }

        posts = posts
            .OrderByDescending(post => post.PublishedDate)
            .Take(20)
            .ToList();

        var cacheDuration = GetCacheDuration();
        _cacheService.Set(CacheKey, posts, cacheDuration);
        await SaveDiskCacheAsync(posts, cacheDuration);

        return posts;
    }

    private TimeSpan GetCacheDuration()
    {
        var minutes = _options.CacheMinutes > 0 ? _options.CacheMinutes : 30;
        var cappedMinutes = Math.Min(minutes, 1440);
        return TimeSpan.FromMinutes(cappedMinutes);
    }

    private async Task<List<XPost>?> TryGetDiskCacheAsync()
    {
        try
        {
            if (!File.Exists(_cacheFilePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(_cacheFilePath);
            var payload = JsonSerializer.Deserialize<XPostsCachePayload>(json, JsonOptions);
            if (payload == null || payload.Posts.Count == 0)
            {
                return null;
            }

            var cacheDuration = GetCacheDuration();
            if (DateTimeOffset.UtcNow - payload.FetchedAt > cacheDuration)
            {
                return null;
            }

            return payload.Posts;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading X posts disk cache");
            return null;
        }
    }

    private async Task SaveDiskCacheAsync(List<XPost> posts, TimeSpan cacheDuration)
    {
        try
        {
            if (cacheDuration <= TimeSpan.Zero)
            {
                return;
            }

            var payload = new XPostsCachePayload
            {
                FetchedAt = DateTimeOffset.UtcNow,
                Posts = posts
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            await File.WriteAllTextAsync(_cacheFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error writing X posts disk cache");
        }
    }

    private async Task<XUserData?> GetUserAsync(HttpClient httpClient, string username)
    {
        try
        {
            using var response = await httpClient.GetAsync($"users/by/username/{username}?user.fields=profile_image_url,name");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("X API user lookup failed for {Username}: {StatusCode}", username, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var userResponse = JsonSerializer.Deserialize<XUserResponse>(json, JsonOptions);
            return userResponse?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching X user profile for {Username}", username);
            return null;
        }
    }

    private async Task<List<XPost>> GetUserPostsAsync(HttpClient httpClient, string userId, XAccount account, string profileImageUrl, int maxResults)
    {
        try
        {
            var url = $"users/{userId}/tweets?max_results={maxResults}&exclude=retweets,replies&tweet.fields=created_at,attachments&expansions=attachments.media_keys&media.fields=url,preview_image_url,type,variants";
            using var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("X API tweets fetch failed for {Username}: {StatusCode}", account.Username, response.StatusCode);
                return [];
            }

            var json = await response.Content.ReadAsStringAsync();
            var tweetResponse = JsonSerializer.Deserialize<XTweetResponse>(json, JsonOptions);

            if (tweetResponse?.Data == null || tweetResponse.Data.Count == 0)
            {
                return [];
            }

            var mediaLookup = tweetResponse.Includes?.Media?
                .Where(media => !string.IsNullOrWhiteSpace(media.MediaKey))
                .ToDictionary(
                    media => media.MediaKey,
                    media => media,
                    StringComparer.OrdinalIgnoreCase);

            var posts = new List<XPost>();
            foreach (var tweet in tweetResponse.Data)
            {
                var mediaUrl = string.Empty;
                var mediaType = string.Empty;
                var videoUrl = string.Empty;
                var videoContentType = string.Empty;
                var videoPreviewImageUrl = string.Empty;
                if (tweet.Attachments?.MediaKeys is { Count: > 0 } mediaKeys && mediaLookup != null)
                {
                    foreach (var mediaKey in mediaKeys)
                    {
                        if (mediaLookup.TryGetValue(mediaKey, out var foundMedia))
                        {
                            mediaType = foundMedia.Type ?? string.Empty;
                            if (string.Equals(mediaType, "video", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(mediaType, "animated_gif", StringComparison.OrdinalIgnoreCase))
                            {
                                videoPreviewImageUrl = foundMedia.PreviewImageUrl ?? string.Empty;
                                var orderedVariants = foundMedia.Variants?
                                    .Where(variant => !string.IsNullOrWhiteSpace(variant.Url))
                                    .OrderByDescending(variant => variant.BitRate ?? 0)
                                    .ToList();

                                var mp4Variant = orderedVariants?.FirstOrDefault(variant => string.Equals(variant.ContentType, "video/mp4", StringComparison.OrdinalIgnoreCase));
                                var preferredVariant = mp4Variant ?? orderedVariants?.FirstOrDefault();

                                if (preferredVariant != null)
                                {
                                    videoUrl = preferredVariant.Url ?? string.Empty;
                                    videoContentType = preferredVariant.ContentType ?? string.Empty;
                                }
                            }
                            else
                            {
                                mediaUrl = foundMedia.Url ?? foundMedia.PreviewImageUrl ?? string.Empty;
                            }

                            if (!string.IsNullOrWhiteSpace(videoUrl) || !string.IsNullOrWhiteSpace(mediaUrl))
                            {
                                break;
                            }
                        }
                    }
                }

                posts.Add(new XPost
                {
                    Id = tweet.Id,
                    Text = tweet.Text,
                    PublishedDate = tweet.CreatedAt,
                    Author = account.DisplayName,
                    Username = account.Username,
                    Url = $"https://x.com/{account.Username}/status/{tweet.Id}",
                    ProfileImageUrl = profileImageUrl ?? string.Empty,
                    MediaUrl = mediaUrl,
                    MediaType = mediaType,
                    VideoUrl = videoUrl,
                    VideoPreviewImageUrl = videoPreviewImageUrl,
                    VideoContentType = videoContentType,
                    Source = "X"
                });
            }

            return posts;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching X posts for {Username}", account.Username);
            return [];
        }
    }

    private sealed record XAccount(string Username, string DisplayName);

    private sealed class XUserResponse
    {
        [JsonPropertyName("data")]
        public XUserData? Data { get; set; }
    }

    private sealed class XUserData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("profile_image_url")]
        public string ProfileImageUrl { get; set; } = string.Empty;
    }

    private sealed class XTweetResponse
    {
        [JsonPropertyName("data")]
        public List<XTweet> Data { get; set; } = [];

        [JsonPropertyName("includes")]
        public XIncludes? Includes { get; set; }
    }

    private sealed class XTweet
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("attachments")]
        public XAttachments? Attachments { get; set; }
    }

    private sealed class XAttachments
    {
        [JsonPropertyName("media_keys")]
        public List<string> MediaKeys { get; set; } = [];
    }

    private sealed class XIncludes
    {
        [JsonPropertyName("media")]
        public List<XMedia> Media { get; set; } = [];
    }

    private sealed class XMedia
    {
        [JsonPropertyName("media_key")]
        public string MediaKey { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("preview_image_url")]
        public string? PreviewImageUrl { get; set; }

        [JsonPropertyName("variants")]
        public List<XMediaVariant>? Variants { get; set; }
    }

    private sealed class XMediaVariant
    {
        [JsonPropertyName("bit_rate")]
        public int? BitRate { get; set; }

        [JsonPropertyName("content_type")]
        public string? ContentType { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    private sealed class XPostsCachePayload
    {
        [JsonPropertyName("fetched_at")]
        public DateTimeOffset FetchedAt { get; set; }

        [JsonPropertyName("posts")]
        public List<XPost> Posts { get; set; } = [];
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using VeilleNet.Models;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.News;

public interface IRedditService
{
    Task<List<RedditPost>> GetTopPostsAsync();
}

public class RedditService : IRedditService
{
    private const string CacheKey = "RedditTopPosts";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] InvalidThumbnails =
        ["self", "default", "nsfw", "spoiler", "image", ""];

    private readonly string[] _subreddits = ["csharp", "dotnet"];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cacheService;
    private readonly RedditOptions _options;
    private readonly ILogger<RedditService> _logger;

    public RedditService(
        IHttpClientFactory httpClientFactory,
        ICacheService cacheService,
        IOptions<RedditOptions> options,
        ILogger<RedditService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cacheService = cacheService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<RedditPost>> GetTopPostsAsync()
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Reddit feature is disabled in configuration.");
            return [];
        }

        var cached = _cacheService.Get<List<RedditPost>>(CacheKey);
        if (cached != null)
        {
            return cached;
        }

        var limit = _options.PostsPerSubreddit > 0 ? _options.PostsPerSubreddit : 10;
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ContainSharp/1.0");

        var tasks = _subreddits.Select(sub => FetchSubredditAsync(httpClient, sub, limit));
        var results = await Task.WhenAll(tasks);

        var posts = results
            .SelectMany(p => p)
            .OrderByDescending(p => p.Score)
            .Take(20)
            .ToList();

        var cacheDuration = TimeSpan.FromMinutes(
            _options.CacheMinutes > 0 ? _options.CacheMinutes : 60);
        _cacheService.Set(CacheKey, posts, cacheDuration);

        return posts;
    }

    private async Task<List<RedditPost>> FetchSubredditAsync(HttpClient client, string subreddit, int limit)
    {
        try
        {
            var url = $"https://www.reddit.com/r/{subreddit}/top.json?t=day&limit={limit}";
            using var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Reddit API returned {StatusCode} for r/{Subreddit}", response.StatusCode, subreddit);
                return [];
            }

            var json = await response.Content.ReadAsStringAsync();
            var listing = JsonSerializer.Deserialize<RedditListing>(json, JsonOptions);

            if (listing?.Data?.Children == null)
            {
                return [];
            }

            var posts = new List<RedditPost>();
            foreach (var child in listing.Data.Children)
            {
                var data = child.Data;
                if (data == null) continue;

                var thumbnail = IsValidThumbnail(data.Thumbnail) ? data.Thumbnail : null;

                posts.Add(new RedditPost
                {
                    Id = data.Id ?? string.Empty,
                    Title = data.Title ?? string.Empty,
                    Text = data.Selftext ?? string.Empty,
                    Url = $"https://www.reddit.com{data.Permalink}",
                    Author = $"u/{data.Author}",
                    Subreddit = data.SubredditNamePrefixed ?? $"r/{subreddit}",
                    Score = data.Score,
                    NumComments = data.NumComments,
                    PublishedDate = DateTimeOffset.FromUnixTimeSeconds((long)data.CreatedUtc).UtcDateTime,
                    Thumbnail = thumbnail
                });
            }

            return posts;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching posts from r/{Subreddit}", subreddit);
            return [];
        }
    }

    private static bool IsValidThumbnail(string? thumbnail)
    {
        if (string.IsNullOrWhiteSpace(thumbnail)) return false;
        if (InvalidThumbnails.Contains(thumbnail, StringComparer.OrdinalIgnoreCase)) return false;
        return thumbnail.StartsWith("http", StringComparison.OrdinalIgnoreCase);
    }

    // Reddit JSON response models
    private sealed class RedditListing
    {
        [JsonPropertyName("data")]
        public RedditListingData? Data { get; set; }
    }

    private sealed class RedditListingData
    {
        [JsonPropertyName("children")]
        public List<RedditChild>? Children { get; set; }
    }

    private sealed class RedditChild
    {
        [JsonPropertyName("data")]
        public RedditPostData? Data { get; set; }
    }

    private sealed class RedditPostData
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("selftext")]
        public string? Selftext { get; set; }

        [JsonPropertyName("permalink")]
        public string? Permalink { get; set; }

        [JsonPropertyName("author")]
        public string? Author { get; set; }

        [JsonPropertyName("subreddit_name_prefixed")]
        public string? SubredditNamePrefixed { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("num_comments")]
        public int NumComments { get; set; }

        [JsonPropertyName("created_utc")]
        public double CreatedUtc { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }
    }
}

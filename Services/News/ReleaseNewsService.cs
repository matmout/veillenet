using System.Text.Json;
using System.Text.Json.Serialization;
using VeilleNet.Models;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.News;

public interface IReleaseNewsService
{
    Task<List<ReleaseNews>> GetLatestReleasesAsync();
}

public class ReleaseNewsService : IReleaseNewsService
{
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReleaseNewsService> _logger;
    private const string CacheKey = "ReleaseNews";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(6);

    // GitHub repos to track for release notes
    private static readonly (string Owner, string Repo, string Framework)[] TrackedRepos =
    [
        ("dotnet", "runtime", ".NET Runtime"),
        ("dotnet", "aspnetcore", "ASP.NET Core"),
        ("dotnet", "efcore", "Entity Framework Core"),
        ("dotnet", "roslyn", "Roslyn (C# Compiler)"),
        ("dotnet", "maui", ".NET MAUI"),
        ("dotnet", "aspire", ".NET Aspire"),
    ];

    public ReleaseNewsService(
        ICacheService cacheService,
        IHttpClientFactory httpClientFactory,
        ILogger<ReleaseNewsService> logger)
    {
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<ReleaseNews>> GetLatestReleasesAsync()
    {
        var cached = _cacheService.Get<List<ReleaseNews>>(CacheKey);
        if (cached != null)
            return cached;

        var allReleases = new List<ReleaseNews>();

        foreach (var (owner, repo, framework) in TrackedRepos)
        {
            try
            {
                var releases = await FetchGitHubReleasesAsync(owner, repo, framework);
                allReleases.AddRange(releases);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch releases for {Owner}/{Repo}", owner, repo);
            }
        }

        var sorted = allReleases
            .OrderByDescending(r => r.ReleaseDate)
            .Take(60)
            .ToList();

        _cacheService.Set(CacheKey, sorted, CacheExpiration);
        return sorted;
    }

    private async Task<List<ReleaseNews>> FetchGitHubReleasesAsync(string owner, string repo, string framework)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("VeilleNet/1.0");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");

        var url = $"https://api.github.com/repos/{owner}/{repo}/releases?per_page=10";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var ghReleases = JsonSerializer.Deserialize<List<GitHubRelease>>(json, JsonOptions);

        if (ghReleases is null)
            return [];

        return ghReleases
            .Where(r => !r.Draft)
            .Select(r => new ReleaseNews
            {
                Version = r.TagName ?? r.Name ?? "unknown",
                Title = $"{framework} — {r.Name ?? r.TagName}",
                Description = TruncateBody(r.Body, 500),
                ReleaseDate = r.PublishedAt ?? r.CreatedAt,
                Url = r.HtmlUrl ?? $"https://github.com/{owner}/{repo}/releases",
                Type = ClassifyRelease(r.TagName, r.Prerelease)
            })
            .ToList();
    }

    private static string ClassifyRelease(string? tag, bool prerelease)
    {
        if (prerelease) return "Preview";
        if (tag != null && tag.Contains("-rc", StringComparison.OrdinalIgnoreCase)) return "RC";
        if (tag != null && tag.Contains("-preview", StringComparison.OrdinalIgnoreCase)) return "Preview";
        return "Stable";
    }

    private static string TruncateBody(string? body, int max)
    {
        if (string.IsNullOrWhiteSpace(body)) return string.Empty;
        // Strip markdown links and keep text
        var clean = System.Text.RegularExpressions.Regex.Replace(body, @"\[([^\]]+)\]\([^\)]+\)", "$1");
        clean = System.Text.RegularExpressions.Regex.Replace(clean, @"[#*`>]", "");
        clean = clean.Replace("\r\n", " ").Replace("\n", " ").Trim();
        return clean.Length > max ? clean[..max] + "..." : clean;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Minimal GitHub Release DTO
    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime? PublishedAt { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }
    }
}

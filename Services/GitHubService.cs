using System.Text.Json;
using VeilleNet.Models;

namespace VeilleNet.Services;

public interface IGitHubService
{
    Task<List<GitHubProject>> GetTrendingCSharpProjectsAsync();
}

public class GitHubService : IGitHubService
{
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string CacheKey = "GitHubTrending";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(6);

    public GitHubService(ICacheService cacheService, IHttpClientFactory httpClientFactory)
    {
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<GitHubProject>> GetTrendingCSharpProjectsAsync()
    {
        var cachedProjects = _cacheService.Get<List<GitHubProject>>(CacheKey);
        if (cachedProjects != null)
        {
            return cachedProjects;
        }

        var projects = new List<GitHubProject>();

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "VeilleNet-Dashboard");

            // Search for trending C# projects (stars in last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            var query = $"language:csharp created:>{thirtyDaysAgo}";
            var searchUrl = $"https://api.github.com/search/repositories?q={Uri.EscapeDataString(query)}&sort=stars&order=desc&per_page=15";

            var response = await httpClient.GetStringAsync(searchUrl);
            var jsonDoc = JsonDocument.Parse(response);

            if (jsonDoc.RootElement.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    projects.Add(new GitHubProject
                    {
                        Name = item.GetProperty("name").GetString() ?? "",
                        FullName = item.GetProperty("full_name").GetString() ?? "",
                        Description = item.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                        Url = item.GetProperty("html_url").GetString() ?? "",
                        Stars = item.GetProperty("stargazers_count").GetInt32(),
                        Forks = item.GetProperty("forks_count").GetInt32(),
                        Language = item.TryGetProperty("language", out var lang) ? lang.GetString() ?? "C#" : "C#",
                        UpdatedAt = DateTime.Parse(item.GetProperty("updated_at").GetString() ?? DateTime.UtcNow.ToString())
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Impossible de récupérer les projets GitHub trending C# Erreur : {ex.Message}", ex);
            // Log error and return empty list or cached data
        }

        _cacheService.Set(CacheKey, projects, CacheExpiration);
        return projects;
    }
}

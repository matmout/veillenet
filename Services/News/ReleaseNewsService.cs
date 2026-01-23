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
    private const string CacheKey = "ReleaseNews";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromDays(1);

    public ReleaseNewsService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<List<ReleaseNews>> GetLatestReleasesAsync()
    {
        var cachedReleases = _cacheService.Get<List<ReleaseNews>>(CacheKey);
        if (cachedReleases != null)
        {
            return cachedReleases;
        }

        // For now, return static data. In production, this would fetch from GitHub API or official .NET feeds
        var releases = new List<ReleaseNews>
        {
            new ReleaseNews
            {
                Version = ".NET 10.0",
                Title = ".NET 10 Stable Release",
                Description = "Official stable release of .NET 10 (LTS) with performance improvements and new C# features",
                ReleaseDate = new DateTime(2025, 11, 12), // Ajustez à la date exacte si nécessaire
                Url = "https://dotnet.microsoft.com/download/dotnet/10.0",
                Type = "Stable"
            },
            new ReleaseNews
            {
                Version = "GPT Codex 5.2",
                Title = "GPT Codex 5.2 Release",
                Description = "Agentic Coding, Large-scale Refactoring, Cybersecurity Defense, Long-horizon tasks.",
                ReleaseDate = new DateTime(2024, 11, 12),
                Url = "https://openai.com/index/introducing-gpt-5-2/",
                Type = "Stable"
            },
            new ReleaseNews
            {
                Version = "C# 14",
                Title = "C# 14 Language Features",
                Description = "New language features including extension member, field and more improvements",
                ReleaseDate = new DateTime(2025, 11, 18),
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14",
                Type = "Stable"
            },
            new ReleaseNews
            {
                Version = "Visual Studio 2026",
                Title = "Latest version Visual Studio",
                Description = "The new IDE brings all AI tools. Dream big. Achieve more.",
                ReleaseDate = new DateTime(2025, 12, 20),
                Url = "https://visualstudio.microsoft.com/?icid=SSM_AS_VisualStudio",
                Type = "Stable"
            }
        };

        _cacheService.Set(CacheKey, releases, CacheExpiration);
        return await Task.FromResult(releases);
    }
}

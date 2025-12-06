using VeilleNet.Models;

namespace VeilleNet.Services;

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
                Version = ".NET 9.0",
                Title = ".NET 9 Stable Release",
                Description = "Official stable release of .NET 9 (STS) with platform updates and improvements",
                ReleaseDate = new DateTime(2024, 11, 12),
                Url = "https://dotnet.microsoft.com/download/dotnet/9.0",
                Type = "Stable"
            },
            new ReleaseNews
            {
                Version = "C# 13",
                Title = "C# 13 Language Features",
                Description = "New language features including collection expressions and primary constructors",
                ReleaseDate = new DateTime(2024, 11, 12),
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13",
                Type = "Stable"
            },
            new ReleaseNews
            {
                Version = "ASP.NET Core 9.0",
                Title = "ASP.NET Core 9.0 Release",
                Description = "Enhanced performance, new Blazor features, and improved minimal APIs",
                ReleaseDate = new DateTime(2024, 11, 12),
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-9.0",
                Type = "Stable"
            }
        };

        _cacheService.Set(CacheKey, releases, CacheExpiration);
        return await Task.FromResult(releases);
    }
}

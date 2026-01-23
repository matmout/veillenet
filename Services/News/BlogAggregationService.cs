using System.ServiceModel.Syndication;
using System.Xml;
using VeilleNet.Models;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.News;

public interface IBlogAggregationService
{
    Task<List<BaseNews>> GetLatestPostsAsync();
}

public class BlogAggregationService : IBlogAggregationService
{
    private readonly ICacheService _cacheService;
    private readonly IFeedService _feedService;
    private const string CacheKey = "BlogPosts";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);
    private const string DefaultMicrosoftImage = "/images/mcp/microsoft.png";

    private readonly List<(string Name, string Url, string DefaultImage)> _blogFeeds = new()
    {
        (".NET Blog", "https://devblogs.microsoft.com/dotnet/feed/", DefaultMicrosoftImage),
        ("ASP.NET Blog", "https://devblogs.microsoft.com/dotnet/category/aspnet/feed/", DefaultMicrosoftImage),
        ("Visual Studio Blog", "https://devblogs.microsoft.com/visualstudio/feed/", DefaultMicrosoftImage),
        ("C# Blog", "https://devblogs.microsoft.com/dotnet/category/csharp/feed/", DefaultMicrosoftImage)
    };

    public BlogAggregationService(ICacheService cacheService, IFeedService feedService)
    {
        _cacheService = cacheService;
        _feedService = feedService;
    }

    public async Task<List<BaseNews>> GetLatestPostsAsync()
    {
        var cachedPosts = _cacheService.Get<List<BaseNews>>(CacheKey);
        if (cachedPosts != null)
        {
            return cachedPosts;
        }

        var posts = new List<BaseNews>();

        foreach (var (name, url, defaultImage) in _blogFeeds)
        {
            try
            {
                var feedPosts = await _feedService.FetchNewsFeedAsync(name, url, defaultImage, 10, "Microsoft");
                posts.AddRange(feedPosts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible de lire le contenu {name} {url} Erreur : {ex.Message}", ex);
                // Log error in production, continue with other feeds
            }
        }

        posts = posts.OrderByDescending(p => p.PublishedDate).Take(20).ToList();
        _cacheService.Set(CacheKey, posts, CacheExpiration);

        return posts;
    }

}

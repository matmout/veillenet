using System.ServiceModel.Syndication;
using System.Xml;
using VeilleNet.Models;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.News;

public interface IWinFormNewsService
{
    Task<List<BaseNews>> GetLatestWinFormNewsAsync();
}

public class WinFormNewsService : IWinFormNewsService
{
    private readonly ICacheService _cacheService;
    private readonly IFeedService _feedService;
    private const string CacheKey = "WinFormNews";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    private readonly List<(string Name, string Url, string Category, string DefaultImage)> _winFormFeeds = new()
    {
        //("Microsoft .NET Blog", "https://devblogs.microsoft.com/dotnet/feed/", "Microsoft", "https://devblogs.microsoft.com/wp-content/uploads/sites/10/2019/05/cropped-dotnet-icon-32x32.png"),
        //("Visual Studio Blog", "https://devblogs.microsoft.com/visualstudio/feed/", "Microsoft", "https://devblogs.microsoft.com/wp-content/uploads/sites/4/2019/01/cropped-vs-icon-32x32.png"),
        ("DevExpress", "https://community.devexpress.com/Blogs/MainFeed", "DevExpress", "https://www.devexpress.com/favicon.ico")
    };

    public WinFormNewsService(ICacheService cacheService, IFeedService feedService)
    {
        _cacheService = cacheService;
        _feedService = feedService;
    }

    public async Task<List<BaseNews>> GetLatestWinFormNewsAsync()
    {
        var cachedNews = _cacheService.Get<List<BaseNews>>(CacheKey);
        if (cachedNews != null)
        {
            return cachedNews;
        }

        var winFormNews = new List<BaseNews>();

        foreach (var (name, url, category, defaultImage) in _winFormFeeds)
        {
            try
            {
                var feedNews = await _feedService.FetchNewsFeedAsync(name, url, defaultImage, category, news => true /*IsWinFormRelated(news.Title, news.Summary)*/);
                winFormNews.AddRange(feedNews);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible de lire le contenu WinForm News {name} {url} Erreur : {ex.Message}", ex);
                // Log error in production, continue with other feeds
            }
        }

        winFormNews = winFormNews.OrderByDescending(n => n.PublishedDate).Take(20).ToList();
        _cacheService.Set(CacheKey, winFormNews, CacheExpiration);

        return winFormNews;
    }

    private bool IsWinFormRelated(string title, string summary)
    {
        var keywords = new[] { "winform", "windows forms", "winforms", "windows form", 
                               "system.windows.forms", "devexpress winforms", "form designer",
                               "windows desktop", "desktop application" };
        
        var combinedText = $"{title} {summary}".ToLowerInvariant();
        return keywords.Any(keyword => combinedText.Contains(keyword));
    }
}

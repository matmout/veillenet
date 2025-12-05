using System.ServiceModel.Syndication;
using System.Xml;
using VeilleNet.Models;

namespace VeilleNet.Services;

public interface IWinFormNewsService
{
    Task<List<WinFormNews>> GetLatestWinFormNewsAsync();
}

public class WinFormNewsService : IWinFormNewsService
{
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string CacheKey = "WinFormNews";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    private readonly List<(string Name, string Url, string Category)> _winFormFeeds = new()
    {
        ("Microsoft .NET Blog", "https://devblogs.microsoft.com/dotnet/feed/", "Microsoft"),
        ("Visual Studio Blog", "https://devblogs.microsoft.com/visualstudio/feed/", "Microsoft"),
        ("DevExpress WinForms", "https://community.devexpress.com/blogs/winforms/rss.aspx", "DevExpress")
    };

    public WinFormNewsService(ICacheService cacheService, IHttpClientFactory httpClientFactory)
    {
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<WinFormNews>> GetLatestWinFormNewsAsync()
    {
        var cachedNews = _cacheService.Get<List<WinFormNews>>(CacheKey);
        if (cachedNews != null)
        {
            return cachedNews;
        }

        var winFormNews = new List<WinFormNews>();

        foreach (var (name, url, category) in _winFormFeeds)
        {
            try
            {
                var feedNews = await FetchFeedAsync(name, url, category);
                winFormNews.AddRange(feedNews);
            }
            catch (Exception)
            {
                // Log error in production, continue with other feeds
            }
        }

        winFormNews = winFormNews.OrderByDescending(n => n.PublishedDate).Take(20).ToList();
        _cacheService.Set(CacheKey, winFormNews, CacheExpiration);

        return winFormNews;
    }

    private async Task<List<WinFormNews>> FetchFeedAsync(string source, string feedUrl, string category)
    {
        var news = new List<WinFormNews>();

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            using var stream = await httpClient.GetStreamAsync(feedUrl);
            using var xmlReader = XmlReader.Create(stream);
            
            var feed = SyndicationFeed.Load(xmlReader);

            foreach (var item in feed.Items.Take(10))
            {
                var title = item.Title?.Text ?? "No title";
                var summary = HtmlSanitizer.StripHtml(item.Summary?.Text);
                
                // Filter for WinForm-related content based on keywords
                if (IsWinFormRelated(title, summary))
                {
                    news.Add(new WinFormNews
                    {
                        Title = title,
                        Url = item.Links.FirstOrDefault()?.Uri.ToString() ?? "",
                        Summary = summary,
                        PublishedDate = item.PublishDate.DateTime,
                        Source = source,
                        Category = category
                    });
                }
            }
        }
        catch
        {
            // Handle feed parsing errors
        }

        return news;
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

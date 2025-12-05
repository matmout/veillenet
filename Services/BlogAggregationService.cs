using System.ServiceModel.Syndication;
using System.Xml;
using VeilleNet.Models;

namespace VeilleNet.Services;

public interface IBlogAggregationService
{
    Task<List<BlogPost>> GetLatestPostsAsync();
}

public class BlogAggregationService : IBlogAggregationService
{
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string CacheKey = "BlogPosts";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    private readonly List<(string Name, string Url)> _blogFeeds = new()
    {
        (".NET Blog", "https://devblogs.microsoft.com/dotnet/feed/"),
        ("ASP.NET Blog", "https://devblogs.microsoft.com/dotnet/category/aspnet/feed/"),
        ("Visual Studio Blog", "https://devblogs.microsoft.com/visualstudio/feed/"),
        ("C# Blog", "https://devblogs.microsoft.com/dotnet/category/csharp/feed/")
    };

    public BlogAggregationService(ICacheService cacheService, IHttpClientFactory httpClientFactory)
    {
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<BlogPost>> GetLatestPostsAsync()
    {
        var cachedPosts = _cacheService.Get<List<BlogPost>>(CacheKey);
        if (cachedPosts != null)
        {
            return cachedPosts;
        }

        var posts = new List<BlogPost>();

        foreach (var (name, url) in _blogFeeds)
        {
            try
            {
                var feedPosts = await FetchFeedAsync(name, url);
                posts.AddRange(feedPosts);
            }
            catch (Exception)
            {
                // Log error in production, continue with other feeds
            }
        }

        posts = posts.OrderByDescending(p => p.PublishedDate).Take(20).ToList();
        _cacheService.Set(CacheKey, posts, CacheExpiration);

        return posts;
    }

    private async Task<List<BlogPost>> FetchFeedAsync(string source, string feedUrl)
    {
        var posts = new List<BlogPost>();

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            using var stream = await httpClient.GetStreamAsync(feedUrl);
            using var xmlReader = XmlReader.Create(stream);
            
            var feed = SyndicationFeed.Load(xmlReader);

            foreach (var item in feed.Items.Take(10))
            {
                posts.Add(new BlogPost
                {
                    Title = item.Title?.Text ?? "No title",
                    Url = item.Links.FirstOrDefault()?.Uri.ToString() ?? "",
                    Summary = HtmlSanitizer.StripHtml(item.Summary?.Text),
                    PublishedDate = item.PublishDate.DateTime,
                    Author = item.Authors.FirstOrDefault()?.Name ?? "Unknown",
                    Source = source
                });
            }
        }
        catch
        {
            // Handle feed parsing errors
        }

        return posts;
    }
}

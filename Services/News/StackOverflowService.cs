using System.ServiceModel.Syndication;
using System.Xml;
using VeilleNet.Models;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.News;

public interface IStackOverflowService
{
    Task<List<StackOverflowQuestion>> GetLatestQuestionsAsync();
}

public class StackOverflowService : IStackOverflowService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICacheService _cacheService;
    private readonly ILogger<StackOverflowService> _logger;
    private const string CacheKey = "StackOverflowQuestions";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);
    private const string FeedUrl = "https://stackoverflow.com/feeds/tag/c%23";// "https://stackoverflow.com/feeds/tag?tagnames=c%23&sort=votes";

    public StackOverflowService(IHttpClientFactory httpClientFactory, ICacheService cacheService, ILogger<StackOverflowService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<StackOverflowQuestion>> GetLatestQuestionsAsync()
    {
        var cachedQuestions = _cacheService.Get<List<StackOverflowQuestion>>(CacheKey);
        if (cachedQuestions != null)
        {
            return cachedQuestions;
        }

        var questions = new List<StackOverflowQuestion>();

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "VeilleNet/1.0");

            using var stream = await httpClient.GetStreamAsync("https://stackoverflow.com/feeds/tag?tagnames=c%23&sort=newest");//;FeedUrl);
            using var xmlReader = XmlReader.Create(stream);
            
            var feed = SyndicationFeed.Load(xmlReader);

            foreach (var item in feed.Items.Take(10))
            {
                var title = item.Title?.Text ?? "No title";
                var summary = HtmlSanitizer.StripHtml(item.Summary?.Text);
                var author = item.Authors.FirstOrDefault()?.Name ?? "Anonymous";
                
                // Extract metadata from item
                var tags = new List<string>();

                // Extract tags from categories
                foreach (var category in item.Categories)
                {
                    if (!string.IsNullOrEmpty(category.Name))
                    {
                        tags.Add(category.Name);
                    }
                }

                var question = new StackOverflowQuestion
                {
                    Title = title,
                    Url = item.Links.FirstOrDefault()?.Uri.ToString() ?? "",
                    Summary = summary,
                    PublishedDate = item.PublishDate.DateTime,
                    Author = author,
                    Tags = tags
                };

                questions.Add(question);
            }

            _cacheService.Set(CacheKey, questions, CacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching StackOverflow questions");
        }

        return questions;
    }
}

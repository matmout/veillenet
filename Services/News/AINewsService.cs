using System.ServiceModel.Syndication;
using System.Xml;
using VeilleNet.Models;
using VeilleNet.Services.Tools;
using VeilleNet.Services.Data;

namespace VeilleNet.Services.News;

public interface IAINewsService
{
    Task<List<BaseNews>> GetLatestAINewsAsync();
}

public class AINewsService : IAINewsService
{
    private readonly ICacheService _cacheService;
    private readonly IFeedService _feedService;
    private const string CacheKey = "AINews";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    private readonly List<(string Name, string Url, string Category, string DefaultImage)> _aiFeeds = new()
    {
        ("OpenAI Blog", "https://openai.com/blog/rss.xml", "AI/ML", string.Empty),
        ("GitHub Blog", "https://github.blog/feed/", "Development", "https://github.githubassets.com/favicons/favicon.png"),
        ("Google AI Blog", "https://feeds.feedburner.com/blogspot/gJZg", "AI/ML", "https://www.gstatic.com/images/branding/product/2x/googleg_32dp.png"),
        ("Hugging Face Blog", "https://huggingface.co/blog/feed.xml", "AI/ML", "https://huggingface.co/favicon.ico"),
        ("Microsoft AI Blog", "https://blogs.microsoft.com/ai/feed/", "AI/ML", "https://blogs.microsoft.com/wp-content/uploads/prod/2019/01/cropped-microsoft_logo_element-192x192.png"),
        ("The Verge AI", "https://www.theverge.com/rss/ai-artificial-intelligence/index.xml", "AI/ML", "https://www.theverge.com/static-assets/icons/favicon.ico"),
        ("NVIDIA AI Blog", "https://blogs.nvidia.com/feed/", "AI/ML", "https://www.nvidia.com/favicon.ico"),
        ("DeepMind Blog", "https://www.deepmind.com/blog/rss.xml", "AI/ML", "https://www.deepmind.com/favicon.ico"),
       // ("Google Developers Tool","https://blog.google/innovation-and-ai/technology/developers-tools/rss/","AI/Tool",string.Empty)
    };

    private readonly INewsRepository _newsRepository;

    public AINewsService(ICacheService cacheService, IFeedService feedService, INewsRepository newsRepository)
    {
        _cacheService = cacheService;
        _feedService = feedService;
        _newsRepository = newsRepository;
    }

    public async Task<List<BaseNews>> GetLatestAINewsAsync()
    {
        var cachedNews = _cacheService.Get<List<BaseNews>>(CacheKey);
        if (cachedNews != null)
        {
            return cachedNews;
        }

        var aiNews = new List<BaseNews>();

        foreach (var (name, url, category, defaultImage) in _aiFeeds)
        {
            try
            {
                var feedNews = await _feedService.FetchNewsFeedAsync(name, url, defaultImage, 10, category, news => IsAIRelated(news.Title, news.Summary));
                aiNews.AddRange(feedNews);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible de lire le contenu AI News {name} {url} Erreur : {ex.Message}", ex);
                // Log error in production, continue with other feeds
            }
        }

        aiNews = aiNews.OrderByDescending(n => n.PublishedDate).Take(20).ToList();
        
        // Enrich with HasAiSummary (optimization: batch check)
        var urls = aiNews.Select(n => n.Url).ToList();
        var existingSummaries = await _newsRepository.GetExistingAiSummaryUrlsAsync(urls);
        
        foreach (var news in aiNews)
        {
            news.HasAiSummary = existingSummaries.Contains(news.Url);
        }

        _cacheService.Set(CacheKey, aiNews, CacheExpiration);

        return aiNews;
    }



    private bool IsAIRelated(string title, string summary)
    {
        var keywords = new[] { "ai", "copilot", "codex", "gpt", "openai", "anthropic", "claude", 
                               "mistral", "llm", "machine learning", "artificial intelligence", 
                               "generative", "code generation", "code assistant", "hugging face",
                               "transformer", "language model", "chatbot", "chatgpt" };
        
        var combinedText = $"{title} {summary}".ToLowerInvariant();
        return keywords.Any(keyword => combinedText.Contains(keyword));
    }
}

using System.ServiceModel.Syndication;
using System.Xml;
using VeilleNet.Models;

namespace VeilleNet.Services;

public interface IAINewsService
{
    Task<List<AINews>> GetLatestAINewsAsync();
}

public class AINewsService : IAINewsService
{
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string CacheKey = "AINews";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    private readonly List<(string Name, string Url, string Category)> _aiFeeds = new()
    {
        ("OpenAI Blog", "https://openai.com/blog/rss.xml", "AI/ML"),
        ("GitHub Blog", "https://github.blog/feed/", "Development"),
        ("Google AI Blog", "https://feeds.feedburner.com/blogspot/gJZg", "AI/ML"),//https://blog.research.google/feeds/posts/default
        ("Hugging Face Blog", "https://huggingface.co/blog/feed.xml", "AI/ML"),
        ("Microsoft AI Blog", "https://blogs.microsoft.com/ai/feed/", "AI/ML"),
        ("The Verge AI","https://www.theverge.com/rss/ai-artificial-intelligence/index.xml","AI/ML"),
        ("NVIDIA AI Blog", "https://blogs.nvidia.com/feed/", "AI/ML"),
        ("DeepMind Blog", "https://www.deepmind.com/blog/rss.xml", "AI/ML")

    };

    public AINewsService(ICacheService cacheService, IHttpClientFactory httpClientFactory)
    {
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<AINews>> GetLatestAINewsAsync()
    {
        var cachedNews = _cacheService.Get<List<AINews>>(CacheKey);
        if (cachedNews != null)
        {
            return cachedNews;
        }

        var aiNews = new List<AINews>();

        foreach (var (name, url, category) in _aiFeeds)
        {
            try
            {
                var feedNews = await FetchFeedAsync(name, url, category);
                aiNews.AddRange(feedNews);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible de lire le contenu AI News {name} {url} Erreur : {ex.Message}", ex);
                // Log error in production, continue with other feeds
            }
        }

        aiNews = aiNews.OrderByDescending(n => n.PublishedDate).Take(20).ToList();
        _cacheService.Set(CacheKey, aiNews, CacheExpiration);

        return aiNews;
    }

    private async Task<List<AINews>> FetchFeedAsync(string source, string feedUrl, string category)
    {
        var news = new List<AINews>();

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "VeilleNet/1.0");
            
            using var stream = await httpClient.GetStreamAsync(feedUrl);
            using var xmlReader = XmlReader.Create(stream);
            
            var feed = SyndicationFeed.Load(xmlReader);

            foreach (var item in feed.Items.Take(10))
            {
                var title = item.Title?.Text ?? "No title";
                var summary = HtmlSanitizer.StripHtml(item.Summary?.Text);
                
                // Filter for AI-related content based on keywords
                if (IsAIRelated(title, summary))
                {
                    news.Add(new AINews
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
        catch (Exception ex)
        {
            Console.WriteLine($"Impossible de parser le feed AI {source} {feedUrl} Erreur : {ex.Message}", ex);
            // Handle feed parsing errors
        }

        return news;
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

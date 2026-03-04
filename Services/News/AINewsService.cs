using VeilleNet.Models;
using VeilleNet.Services.Data;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.News;

public interface IAINewsService
{
    Task<List<BaseNews>> GetLatestAINewsAsync();
}

public class AINewsService : BaseNewsAggregationService, IAINewsService
{
    private static readonly string[] AiKeywords =
    [
        "ai", "copilot", "codex", "gpt", "openai", "anthropic", "claude",
        "mistral", "llm", "machine learning", "artificial intelligence",
        "generative", "code generation", "code assistant", "hugging face",
        "transformer", "language model", "chatbot", "chatgpt"
    ];

    protected override string CacheKey => "AINews";

    protected override IReadOnlyList<FeedDefinition> Feeds { get; } = new List<FeedDefinition>
    {
        new("OpenAI Blog", "https://openai.com/blog/rss.xml", "/images/mcp/openai.png", "AI/ML", Filter: n => IsAIRelated(n.Title, n.Summary)),
        new("GitHub Blog", "https://github.blog/feed/", "https://github.githubassets.com/favicons/favicon.png", "Development", Filter: n => IsAIRelated(n.Title, n.Summary)),
        new("Google AI Blog", "https://feeds.feedburner.com/blogspot/gJZg", "https://www.gstatic.com/images/branding/product/2x/googleg_32dp.png", "AI/ML", Filter: n => IsAIRelated(n.Title, n.Summary)),
        new("Hugging Face Blog", "https://huggingface.co/blog/feed.xml", "https://huggingface.co/favicon.ico", "AI/ML", Filter: n => IsAIRelated(n.Title, n.Summary)),
        new("Microsoft AI Blog", "https://blogs.microsoft.com/ai/feed/", "https://blogs.microsoft.com/wp-content/uploads/prod/2019/01/cropped-microsoft_logo_element-192x192.png", "AI/ML", Filter: n => IsAIRelated(n.Title, n.Summary)),
        new("The Verge AI", "https://www.theverge.com/rss/ai-artificial-intelligence/index.xml", "https://www.theverge.com/static-assets/icons/favicon.ico", "AI/ML", Filter: n => IsAIRelated(n.Title, n.Summary)),
        new("NVIDIA AI Blog", "https://blogs.nvidia.com/feed/", "https://www.nvidia.com/favicon.ico", "AI/ML", Filter: n => IsAIRelated(n.Title, n.Summary)),
        new("DeepMind Blog", "https://www.deepmind.com/blog/rss.xml", "https://www.deepmind.com/favicon.ico", "AI/ML", Filter: n => IsAIRelated(n.Title, n.Summary)),
    };

    public AINewsService(
        ICacheService cacheService,
        IFeedService feedService,
        IAiSummaryRepository aiSummaryRepository,
        ILogger<AINewsService> logger)
        : base(cacheService, feedService, aiSummaryRepository, logger) { }

    public Task<List<BaseNews>> GetLatestAINewsAsync() => FetchAndCacheAsync();

    private static bool IsAIRelated(string title, string summary)
    {
        var combinedText = $"{title} {summary}".ToLowerInvariant();
        return AiKeywords.Any(keyword => combinedText.Contains(keyword));
    }
}

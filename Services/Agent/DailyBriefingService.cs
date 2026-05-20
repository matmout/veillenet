using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using VeilleNet.Models;
using VeilleNet.Services.Data;
using VeilleNet.Services.News;

namespace VeilleNet.Services.Agent;

public interface IDailyBriefingService
{
    Task<string?> GetOrGenerateDailyBriefingAsync(CancellationToken cancellationToken = default);
}

public class DailyBriefingService : IDailyBriefingService
{
    private readonly IAiSummaryRepository _aiSummaryRepository;
    private readonly IArticleRepository _articleRepository;
    private readonly IRedditService _redditService;
    private readonly IMistralChatClientFactory _chatClientFactory;
    private readonly MistralOptions _mistralOptions;
    private readonly RedditOptions _redditOptions;
    private readonly ILogger<DailyBriefingService> _logger;

    public DailyBriefingService(
        IAiSummaryRepository aiSummaryRepository,
        IArticleRepository articleRepository,
        IRedditService redditService,
        IMistralChatClientFactory chatClientFactory,
        IOptions<MistralOptions> mistralOptions,
        IOptions<RedditOptions> redditOptions,
        ILogger<DailyBriefingService> logger)
    {
        _aiSummaryRepository = aiSummaryRepository;
        _articleRepository = articleRepository;
        _redditService = redditService;
        _chatClientFactory = chatClientFactory;
        _mistralOptions = mistralOptions.Value;
        _redditOptions = redditOptions.Value;
        _logger = logger;
    }

    public async Task<string?> GetOrGenerateDailyBriefingAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var existing = await _articleRepository.GetDailyBriefingByDateAsync(today, cancellationToken);
        if (existing != null)
        {
            _logger.LogInformation("Returning cached daily briefing for {Date}", today);
            return existing.Content;
        }

        var chatClient = _chatClientFactory.TryCreate();
        if (chatClient is null)
        {
            _logger.LogWarning("Mistral not configured; skipping daily briefing generation.");
            return null;
        }

        var since = DateTime.UtcNow.AddHours(-24);
        var summaries = await _aiSummaryRepository.GetAiSummariesByDateRangeAsync(since, DateTime.UtcNow, cancellationToken);

        var redditPosts = new List<RedditPost>();
        if (_redditOptions.Enabled)
        {
            try
            {
                redditPosts = await _redditService.GetTopPostsAsync();
                redditPosts = redditPosts
                    .Where(p => p.PublishedDate >= since)
                    .Take(5)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch Reddit posts for daily briefing; continuing without them.");
            }
        }

        if (summaries.Count == 0 && redditPosts.Count == 0)
        {
            _logger.LogWarning("No news available in the last 24h for daily briefing.");
            return null;
        }

        var prompt = BuildPrompt(summaries.Take(8).ToList(), redditPosts);
        _logger.LogInformation("Generating daily briefing from {ArticleCount} articles and {RedditCount} Reddit posts.",
            summaries.Count, redditPosts.Count);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System,
                "You are a .NET technology watch expert. Based on the following news from the last 24 hours, write a concise \"Today in 60 Seconds\" briefing for .NET and C# developers.\n" +
                "Rules:\n" +
                "- Language: English only.\n" +
                "- Format: 3 to 5 bullet points starting with \"•\", each one sentence, max 25 words per bullet.\n" +
                "- Focus on the most impactful or actionable news.\n" +
                "- Be factual and direct. No marketing language.\n" +
                "- Total output: 80 to 150 words maximum.\n" +
                "- Output ONLY the bullet points, no intro or outro."),
            new(ChatRole.User, prompt)
        };

        try
        {
            var chatOptions = new ChatOptions { Temperature = _mistralOptions.Temperature };
            var response = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
            var content = response.Text?.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Mistral returned empty content for daily briefing.");
                return null;
            }

            var totalArticles = summaries.Count + redditPosts.Count;
            await _articleRepository.AddOrUpdateDailyBriefingAsync(today, content, totalArticles, cancellationToken);

            _logger.LogInformation("Daily briefing generated and persisted for {Date}.", today);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily briefing for {Date}.", today);
            return null;
        }
    }

    private static string BuildPrompt(List<Models.Entities.AiSummaryEntity> summaries, List<RedditPost> redditPosts)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Articles (AI summaries) ===");

        foreach (var s in summaries)
        {
            var snippet = s.Summary?.Length > 100
                ? s.Summary[..100].TrimEnd() + "…"
                : s.Summary ?? string.Empty;
            // Strip HTML tags from snippet for a cleaner prompt
            snippet = System.Text.RegularExpressions.Regex.Replace(snippet, "<[^>]+>", " ").Trim();
            sb.AppendLine($"[{s.Source}] {s.Title} — {snippet}");
        }

        if (redditPosts.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("=== Reddit (r/csharp & r/dotnet) ===");
            foreach (var post in redditPosts)
            {
                sb.AppendLine($"[{post.Subreddit}] {post.Title}");
            }
        }

        return sb.ToString();
    }
}

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using VeilleNet.Models;
using VeilleNet.Models.Entities;
using VeilleNet.Services.News;
using VeilleNet.Services.Tools;
using VeilleNet.Services.Data;
using System.Text.RegularExpressions;
using System.Net;

namespace VeilleNet.Services.Agent;

public interface IAiSummarizationService
{
    Task<List<AiContentSummary>> GetLatestBlogSummariesAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<List<AiContentSummary>> GetLatestBlogSummariesFromDatabaseAsync(int count = 10, CancellationToken cancellationToken = default);
    Task BackfillEntitiesAsync(CancellationToken cancellationToken = default);
}

public class AiSummarizationService : IAiSummarizationService
{
    private static int _entitiesBackfillStarted;
    private readonly IBlogAggregationService _blogAggregationService;
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMistralChatClientFactory _chatClientFactory;
    private readonly IAINewsService _aiNewsService;
    private readonly IWinFormNewsService _winFormNewsService;
    private readonly IArticleRepository _articleRepository;
    private readonly IAiSummaryRepository _aiSummaryRepository;
    private readonly MistralOptions _options;
    private readonly ILogger<AiSummarizationService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly INewsDeduplicationService _deduplicationService;

    public AiSummarizationService(
        IBlogAggregationService blogAggregationService,
        ICacheService cacheService,
        IHttpClientFactory httpClientFactory,
        IMistralChatClientFactory chatClientFactory,
        IAINewsService aiNewsService,
        IWinFormNewsService winFormNewsService,
        IArticleRepository articleRepository,
        IAiSummaryRepository aiSummaryRepository,
        IOptions<MistralOptions> options,
        ILogger<AiSummarizationService> logger,
        IServiceScopeFactory scopeFactory,
        INewsDeduplicationService deduplicationService)
    {
        _blogAggregationService = blogAggregationService;
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
        _chatClientFactory = chatClientFactory;
        _aiNewsService = aiNewsService;
        _winFormNewsService = winFormNewsService;
        _articleRepository = articleRepository;
        _aiSummaryRepository = aiSummaryRepository;
        _options = options.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _deduplicationService = deduplicationService;
    }

    public async Task<List<AiContentSummary>> GetLatestBlogSummariesFromDatabaseAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var summaries = await _aiSummaryRepository.GetRecentAiSummariesAsync(count, cancellationToken);
        return summaries.Select(s => s.ToAiContentSummary()).ToList();
    }

    public async Task<List<AiContentSummary>> GetLatestBlogSummariesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var latestNewsThreshold = DateTime.Today.AddDays(-1);
        var fallbackThreshold = DateTime.Today.AddDays(-2);
        List<BaseNews> globalPosts = new List<BaseNews>();
        var posts = (await _blogAggregationService.GetLatestPostsAsync()).Where(w => w.PublishedDate >= latestNewsThreshold).ToList();
        var aiNewsTask = (await _aiNewsService.GetLatestAINewsAsync()).Where(w => w.PublishedDate >= latestNewsThreshold).ToList();
        var winFormTask = (await _winFormNewsService.GetLatestWinFormNewsAsync()).Where(w => w.PublishedDate >= latestNewsThreshold).ToList();

        globalPosts.AddRange(posts);
        globalPosts.AddRange(aiNewsTask);
        globalPosts.AddRange(winFormTask);
        //posts.AddRange(videoTask); TO be implemented after

        // Deduplicate news
        var recentArticles = await _articleRepository.GetRecentNewsArticlesAsync(200, cancellationToken);
        var uniquePosts = new List<BaseNews>();

        foreach (var post in globalPosts)
        {
            if (!_deduplicationService.IsDuplicate(post, recentArticles))
            {
                uniquePosts.Add(post);
            }
            else
            {
                _logger.LogInformation("Skipping duplicate news: {Title} ({Url})", post.Title, post.Url);
            }
        }
        
        globalPosts = uniquePosts;

        if (globalPosts.Count > 0)
        {
            // Save news articles to database
            try
            {
                await _articleRepository.AddOrUpdateNewsArticlesAsync(globalPosts, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving news articles to database");
            }
        }

        if (globalPosts.Count == 0)
        {
            var articlesWithoutSummary = await _articleRepository.GetRecentNewsArticlesWithoutAiSummaryAsync(fallbackThreshold, count, cancellationToken);
            globalPosts = articlesWithoutSummary
                .Select(MapToBaseNews)
                .ToList();

            if (globalPosts.Count > 0)
            {
                _logger.LogInformation(
                    "No unique fresh news found. Retrying {Count} recent persisted news without AI summary since {Threshold}",
                    globalPosts.Count,
                    fallbackThreshold);
            }
        }

        // If not configured, degrade gracefully to RSS summaries.
        var chatClient = _chatClientFactory.TryCreate();
        if (chatClient is null)
        {
            return globalPosts.Select(p => new AiContentSummary
            {
                Title = p.Title,
                Url = p.Url,
                Source = p.Source,
                PublishedDate = p.PublishedDate,
                Summary = p.Summary
            }).ToList();
        }

        using var gate = new SemaphoreSlim(1);
        var tasks = globalPosts.Select(async p =>
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                return await SummarizePostAsync(chatClient, p, cancellationToken);
            }
            finally
            {
                gate.Release();
            }
        });

        var summaries = (await Task.WhenAll(tasks)).Where(s => s is not null).Select(s => s!).ToList();

        // Save AI summaries to database
        try
        {
            await _aiSummaryRepository.AddOrUpdateAiSummariesAsync(summaries, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving AI summaries to database");
        }

        // Trigger backfill if no entities exist, using a background scope to avoid DbContext concurrency issues
        // Guard: only one backfill can run at a time across all instances
        if (Interlocked.CompareExchange(ref _entitiesBackfillStarted, 1, 0) == 0)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scopedRepo = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
                    var scopedAiService = scope.ServiceProvider.GetRequiredService<IAiSummarizationService>();

                    var entityCount = await scopedRepo.GetNamedEntityCountAsync(CancellationToken.None);
                    if (entityCount == 0)
                    {
                        await scopedAiService.BackfillEntitiesAsync(CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error triggering background backfill");
                }
                finally
                {
                    Interlocked.Exchange(ref _entitiesBackfillStarted, 0);
                }
            });
        }

        return summaries;
    }

    public async Task BackfillEntitiesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting backfill for named entities...");
        var chatClient = _chatClientFactory.TryCreate();
        if (chatClient is null) return;

        var articles = await _articleRepository.GetRecentNewsArticlesAsync(30, cancellationToken);
        var processed = 0;
        var skipped = 0;
        foreach (var article in articles)
        {
            try
            {
                // Skip articles that already have entities — no need to call Mistral again
                if (article.Entities != null && article.Entities.Any())
                {
                    skipped++;
                    continue;
                }

                processed++;
                _logger.LogInformation("Backfilling entities for article: {Title}", article.Title);

                var messages = BuildEntityExtractionMessages(article.Title, article.Summary);
                var chatOptions = new ChatOptions { Temperature = _options.Temperature };
                var response = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
                var entities = response.Text?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                if (entities != null && entities.Any())
                {
                    await _articleRepository.AddEntitiesToArticleAsync(article.Id, entities, cancellationToken);
                }

                // Rate limiting: avoid Mistral API burst
                await Task.Delay(500, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error backfilling article: {Id}", article.Id);
            }
        }
        _logger.LogInformation("Backfill completed. Processed={Processed} SkippedAlreadyTagged={Skipped}", processed, skipped);
    }

    private async Task<AiContentSummary?> SummarizePostAsync(IChatClient chatClient, BaseNews post, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(post.Url))
        {
            return null;
        }

        // Check database first
        try
        {
            var existingSummary = await _aiSummaryRepository.GetAiSummaryByUrlAsync(post.Url, cancellationToken);
            if (existingSummary != null && existingSummary.SummaryDate >= DateTime.UtcNow.AddHours(-24))
            {
                _logger.LogInformation("Using cached AI summary from database for: {Url}", post.Url);
                return existingSummary.ToAiContentSummary();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking database for existing summary: {Url}", post.Url);
        }

        var cacheKey = GetCacheKey(post.Url);
        var cached = _cacheService.Get<AiContentSummary>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VeilleNet-AI-Summary");
        httpClient.Timeout = TimeSpan.FromSeconds(15);

        // SSRF protection: validate URL against allowlist before fetching
        if (!UrlSafetyValidator.IsSafeUrl(post.Url))
        {
            var reason = UrlSafetyValidator.GetRejectionReason(post.Url);
            _logger.LogWarning("Blocked unsafe URL for AI summarization: {Url} — Reason: {Reason}", post.Url, reason);
            return null;
        }

        string html = null;
        try
        {
            var response = await httpClient.GetAsync(
                post.Url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            const int maxBytes = 512 * 1024;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var memory = new MemoryStream();

            var buffer = new byte[8192];
            int totalRead = 0;
            int read;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                if (totalRead + read > maxBytes)
                {
                    read = maxBytes - totalRead;
                }

                await memory.WriteAsync(buffer, 0, read, cancellationToken);
                totalRead += read;

                if (totalRead >= maxBytes)
                {
                    _logger.LogWarning("Response truncated at {Size} bytes for URL: {Url}", maxBytes, post.Url);
                    break;
                }
            }

            html = Encoding.UTF8.GetString(memory.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching URL: {Url}", post.Url);
        }

        if (string.IsNullOrEmpty(html))
        {
            return null;
        }

        // Extract article content intelligently using the title as a guide
        var content = ArticleContentExtractor.ExtractArticleContent(html, post.Title);
        
        if (string.IsNullOrWhiteSpace(content))
        {
            // Fallback to simple HTML stripping if intelligent extraction fails
            content = HtmlSanitizer.StripHtml(html);
        }
        
        if (content.Length > _options.MaxInputChars)
        {
            var cutoff = content.LastIndexOf(' ', _options.MaxInputChars - 1);
            content = cutoff > 0
                ? content[..cutoff]
                : content[.._options.MaxInputChars];
        }

        _logger.LogInformation("Summarizing article: {Title} ({Url}) ({Content})", post.Title, post.Url, content);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System,
                "You are a professional .NET technology watch assistant.\n" +
                "Your goal is to produce a concise, expert-level summary of the article below and extract key named entities.\n\n" +
                "Response format (always follow exactly):\n" +
                "[SUMMARY]\n" +
                "(the summary, following the structure below)\n" +
                "[ENTITIES]\n" +
                "(a comma-separated list of 3-7 key technologies, frameworks, or concepts mentioned)\n\n" +
                "Summary structure:\n" +
                "1) One-sentence hook introducing the topic and its relevance to .NET or software engineering.\n" +
                "2) 3-5 bullet points (each on its own line, formatted as \"- \" followed by a strong verb such as \"Explains\", \"Introduces\", \"Analyzes\", \"Compares\", \"Highlights\"), covering the key insights AND their practical impact on daily work, architecture decisions, tooling, or learning priorities.\n" +
                "3) A closing line starting with \"Why it matters:\" followed by one sentence on the strategic importance.\n\n" +
                "Style and constraints:\n" +
                "- Language: English only.\n" +
                "- Tone: professional, concise, factual. No marketing language, no hype.\n" +
                "- Do not hallucinate: if information is missing or unclear, say so explicitly instead of inventing details.\n" +
                "- Keep a consistent tone and structure across articles, but tailor the content to each specific article so summaries don't feel repetitive.\n" +
                "- Emphasize .NET, C#, AI, cloud, architecture, performance, security, or tooling aspects when relevant.\n" +
                "- Maximum length: 150 words. Be shorter if the article has little valuable or novel information.\n" +
                "- Never include code unless essential to understanding the point, and keep any code extremely brief."),
            new(ChatRole.User, $"Title: {post.Title}\nSource: {post.Source}\nURL: {post.Url}\n\nContent (excerpt):\n{content}\n\nSummarize this article following the format and rules defined above.")
        };

        var chatOptions = new ChatOptions
        {
            Temperature = _options.Temperature
        };

        string responseText;
        try
        {
            var response = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
            responseText = response.Text;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error during AI summarization for article: {Title} ({Url})", post.Title, post.Url);
            return null;
        }

        if(string.IsNullOrEmpty(responseText))
        {
            return null;
        }

        var parts = Regex.Split(responseText, @"\[ENTITIES\]", RegexOptions.IgnoreCase);
        var summaryText = Regex.Replace(parts[0], @"\[SUMMARY\]", "", RegexOptions.IgnoreCase).Trim();
        var entitiesText = parts.Length > 1 ? parts[1].Trim() : "";

        if (parts.Length == 1)
        {
            _logger.LogWarning("Mistral response missing [ENTITIES] marker for {Url}; entities will be empty", post.Url);
        }

        var entities = entitiesText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        string htmlReponseText = AddHtmlToText(summaryText);

        // Sanitize HTML output to prevent XSS before storage
        htmlReponseText = HtmlOutputSanitizer.Sanitize(htmlReponseText);

        var result = new AiContentSummary
        {
            Title = post.Title,
            Url = post.Url,
            Source = post.Source,
            PublishedDate = post.PublishedDate,
            Summary = htmlReponseText,
            Entities = entities,
            AiGenerated = true,
            SummaryDate = DateTime.UtcNow
        };

        _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(_options.CacheMinutes));
        return result;
    }

    private static List<ChatMessage> BuildEntityExtractionMessages(string? title, string? summary)
    {
        return new List<ChatMessage>
        {
            new(ChatRole.System, "You are an expert in .NET and software engineering. Extract 3 to 7 key technologies, frameworks, or concepts explicitly mentioned in the provided title and summary. Avoid generic terms like \"software\" or \"technology\". Respond ONLY with a comma-separated list of entities, nothing else."),
            new(ChatRole.User, $"Title: {title}\nSummary: {summary}")
        };
    }

    private static string AddHtmlToText(string responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
        {
            return string.Empty;
        }

        // Normalize line endings
        var text = responseText.Replace("\r\n", "\n");

        // First, HTML-encode everything
        text = WebUtility.HtmlEncode(text);

        // Convert encoded **bold** markers back to <strong>
        // After encoding, ** becomes ** (not changed), so we search on that pattern
        text = Regex.Replace(text, @"\*\*(.+?)\*\*", m => $"<strong>{m.Groups[1].Value}</strong>");

        // Convert consecutive "- " bullet lines into a real <ul><li> list instead of
        // leaving them as plain dashes separated by <br />
        var lines = text.Split('\n');
        var rebuilt = new StringBuilder();
        var inList = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("- "))
            {
                if (!inList)
                {
                    rebuilt.Append("<ul>");
                    inList = true;
                }

                rebuilt.Append("<li>").Append(line[2..]).Append("</li>");
                continue;
            }

            if (inList)
            {
                rebuilt.Append("</ul>\n");
                inList = false;
            }

            rebuilt.Append(line).Append('\n');
        }

        if (inList)
        {
            rebuilt.Append("</ul>");
        }

        text = rebuilt.ToString().TrimEnd('\n');

        // Replace remaining newlines with <br /> for email / HTML
        text = text.Replace("\n\n", "<br /><br />");
        text = text.Replace("\n", "<br />");

        // Wrap in a simple container for styling if needed
        return "<div class=\"ai-summary\">" + text + "</div>";
    }

    private static BaseNews MapToBaseNews(NewsArticle article)
    {
        return new BaseNews
        {
            Title = article.Title,
            Url = article.Url,
            Summary = article.Summary,
            PublishedDate = article.PublishedDate,
            Author = article.Author,
            Source = article.Source,
            Category = article.Category,
            Image = article.Image
        };
    }

    private static string GetCacheKey(string url)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return "AiSummary:" + Convert.ToHexString(hash);
    }

}

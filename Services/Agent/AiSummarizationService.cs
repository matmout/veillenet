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
    Task<string?> GetDominantThemeFromRecentNewsAsync(CancellationToken cancellationToken = default);
    Task BackfillEntitiesAsync(CancellationToken cancellationToken = default);
    Task BackfillKeywordsForLastAiSummarizedNewsOnceAsync(int count = 100, CancellationToken cancellationToken = default);
}

public class AiSummarizationService : IAiSummarizationService
{
    private static int _keywordsBackfillStarted;
    private static int _entitiesBackfillStarted;
    private readonly IBlogAggregationService _blogAggregationService;
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMistralChatClientFactory _chatClientFactory;
    private readonly IAINewsService _aiNewsService;
    private readonly IWinFormNewsService _winFormNewsService;
    private readonly IVideoService _videoService;
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
        IVideoService videoService,
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
        _videoService = videoService;
        _articleRepository = articleRepository;
        _aiSummaryRepository = aiSummaryRepository;
        _options = options.Value;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _deduplicationService = deduplicationService;
    }

    public async Task BackfillKeywordsForLastAiSummarizedNewsOnceAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _keywordsBackfillStarted, 1) == 1)
        {
            return;
        }

        _logger.LogInformation("Starting one-shot keyword backfill for last {Count} AI summarized news...", count);

        var chatClient = _chatClientFactory.TryCreate();
        if (chatClient is null) return;

        var articles = await _articleRepository.GetRecentAiSummarizedNewsArticlesAsync(count, cancellationToken);
        var skipped = 0;
        var processed = 0;
        foreach (var article in articles)
        {
            try
            {
                if (article.Entities != null && article.Entities.Any())
                {
                    skipped++;
                    continue;
                }

                processed++;

                var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, "You are an expert in .NET and software engineering. Extract 3 to 7 key technologies, frameworks, or concepts mentioned in the provided title and summary as named entities. Respond ONLY with a comma-separated list of entities."),
                    new(ChatRole.User, $"Title: {article.Title}\nSummary: {article.Summary}")
                };

                var response = await chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
                var entities = response.Text?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                // Rate limiting: avoid Mistral API burst
                await Task.Delay(500, cancellationToken);

                if (entities != null && entities.Any())
                {
                    await _articleRepository.AddEntitiesToArticleAsync(article.Id, entities, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error backfilling keywords for article: {Id}", article.Id);
            }
        }

        _logger.LogInformation("One-shot keyword backfill completed. Processed={Processed} SkippedAlreadyTagged={Skipped}", processed, skipped);
    }

    public async Task<List<AiContentSummary>> GetLatestBlogSummariesFromDatabaseAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var summaries = await _aiSummaryRepository.GetRecentAiSummariesAsync(count, cancellationToken);
        return summaries.Select(s => s.ToAiContentSummary()).ToList();
    }

    public async Task<string?> GetDominantThemeFromRecentNewsAsync(CancellationToken cancellationToken = default)
    {
        var generationDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var existingTheme = await _articleRepository.GetDominantThemeByDateAsync(generationDate, cancellationToken);
        if (existingTheme != null)
        {
            return FormatThemeOutput(existingTheme.Theme, existingTheme.Rationale);
        }

        var sinceDate = DateTime.UtcNow.AddDays(-3);
        var articles = await _articleRepository.GetRecentNewsArticlesAsync(200, cancellationToken);
        var recentTitles = articles
            .Where(a => a.PublishedDate >= sinceDate)
            .Select(a => a.Title?.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        if (recentTitles.Count == 0)
        {
            return null;
        }

        var chatClient = _chatClientFactory.TryCreate();
        if (chatClient is null)
        {
            _logger.LogWarning("Chat client not configured; unable to detect dominant theme.");
            return null;
        }

        var titlesBuilder = new StringBuilder();
        for (var i = 0; i < recentTitles.Count; i++)
        {
            titlesBuilder.Append(i + 1).Append(". ").AppendLine(recentTitles[i]!);
        }

        var titlesPayload = titlesBuilder.ToString();
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a technology trend analyst. Given multiple software and AI news headlines, identify the single dominant theme tying most of them together. Respond with 'Theme: <short theme>' on the first line and one concise sentence explaining the rationale."),
            new(ChatRole.User, $"Here are {recentTitles.Count} headlines published within the last 3 days:\n{titlesPayload}\n\nIdentify the theme that appears the most across these headlines.")
        };

        var chatOptions = new ChatOptions { Temperature = _options.Temperature };

        try
        {
            var response = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
            var parsedTheme = ParseDominantThemeResponse(response.Text);

            if (parsedTheme != null)
            {
                await _articleRepository.AddOrUpdateDominantThemeAsync(generationDate, parsedTheme.Value.theme, parsedTheme.Value.rationale, cancellationToken);
                return FormatThemeOutput(parsedTheme.Value.theme, parsedTheme.Value.rationale);
            }

            var fallback = response.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(fallback))
            {
                await _articleRepository.AddOrUpdateDominantThemeAsync(generationDate, fallback!, null, cancellationToken);
            }

            return fallback;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting dominant theme from recent news");
            return null;
        }
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

                var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, "You are an expert in .NET and software engineering. Extract 3 to 7 key technologies, frameworks, or concepts mentioned in the provided title and summary as named entities. Respond ONLY with a comma-separated list of entities."),
                    new(ChatRole.User, $"Title: {article.Title}\nSummary: {article.Summary}")
                };

                var response = await chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
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
            content = content[.._options.MaxInputChars];
        }

        _logger.LogInformation("Summarizing article: {Title} ({Url}) ({Content})", post.Title, post.Url, content);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System,  @"You are a professional .NET technology watch assistant. 
    Your goal is to produce a concise, expert-level summary of each article and extract key named entities.\n\n
    Response format:\n
    [SUMMARY]\n
    (The summary following the architecture below)\n
    [ENTITIES]\n
    (Comma-separated list of 3-7 key technologies, frameworks, or concepts mentioned)\n\n
    Summary architecture (must ALWAYS be the same, even if the content changes):\n
    1) Short context introduction (2–3 sentences) describing the main topic and its relevance to .NET or software engineering.\n
    2) Key insights: 2-4 bullet points, each starting with a strong verb (e.g., 'Explains', 'Introduces', 'Analyzes', 'Compares', 'Highlights').\n
    3) Practical impact: 1–2 sentences explaining how this content can influence daily work, architecture decisions, tooling, or learning priorities.\n
    4) Consistent closing line starting with 'Why it matters:' followed by one sentence that summarizes the strategic importance.\n\n
    Style and constraints:\n
    - Language: English only.\n
    - Tone: professional, concise, factual, no marketing language, no hype.\n
    - Do not hallucinate; if information is missing or unclear, explicitly say what is uncertain instead of inventing details.\n
    - Reuse a consistent wording style and structure every day, but adapt the content to the specific article so that the summary itself is not repetitive.\n
    - Emphasize .NET, C#, IA, cloud, architecture, performance, security, or tooling aspects when relevant.\n
    - Maximum length: 200 words. Be shorter if the article contains little valuable or novel information.\n
    - Never include code unless it is essential to understand the point, and keep any code extremely brief.\n"),
            new(ChatRole.User, $"Titre: {post.Title}\nSource: {post.Source}\nURL: {post.Url}\n\nContenu (extrait):\n{content}\n\nTâche: Résume en 4-6 puces, puis une phrase 'Pourquoi c'est important', et liste les entités.")
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

        var parts = responseText.Split("[ENTITIES]", StringSplitOptions.RemoveEmptyEntries);
        var summaryText = parts[0].Replace("[SUMMARY]", "").Trim();
        var entitiesText = parts.Length > 1 ? parts[1].Trim() : "";

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

    private static (string theme, string? rationale)? ParseDominantThemeResponse(string? responseText)
    {
        if (string.IsNullOrWhiteSpace(responseText))
        {
            return null;
        }

        var normalized = responseText.Trim();
        var lines = normalized
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (lines.Length == 0)
        {
            return null;
        }

        var themeLine = lines[0];
        if (themeLine.StartsWith("Theme:", StringComparison.OrdinalIgnoreCase))
        {
            themeLine = themeLine[6..].Trim();
        }

        if (string.IsNullOrWhiteSpace(themeLine))
        {
            return null;
        }

        var rationale = lines.Length > 1 ? string.Join(' ', lines.Skip(1)).Trim() : null;
        if (string.IsNullOrWhiteSpace(rationale))
        {
            rationale = null;
        }

        return (themeLine, rationale);
    }

    private static string FormatThemeOutput(string theme, string? rationale)
    {
        var normalizedTheme = theme.StartsWith("Theme:", StringComparison.OrdinalIgnoreCase)
            ? theme
            : $"Theme: {theme}";

        return string.IsNullOrWhiteSpace(rationale)
            ? normalizedTheme
            : normalizedTheme + Environment.NewLine + rationale;
    }
}

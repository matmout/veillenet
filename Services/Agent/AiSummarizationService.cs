using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using VeilleNet.Models;
using VeilleNet.Services.News;
using VeilleNet.Services.Tools;
using System.Text.RegularExpressions;
using System.Net;

namespace VeilleNet.Services.Agent;

public interface IAiSummarizationService
{
    Task<List<AiContentSummary>> GetLatestBlogSummariesAsync(int count = 10, CancellationToken cancellationToken = default);
}

public class AiSummarizationService : IAiSummarizationService
{
    private readonly IBlogAggregationService _blogAggregationService;
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMistralChatClientFactory _chatClientFactory;
    private readonly IAINewsService _aiNewsService;
    private readonly IWinFormNewsService _winFormNewsService;
    private readonly IVideoService _videoService;
    private readonly MistralOptions _options;
    private readonly ILogger<AiSummarizationService> _logger;

    public AiSummarizationService(
        IBlogAggregationService blogAggregationService,
        ICacheService cacheService,
        IHttpClientFactory httpClientFactory,
        IMistralChatClientFactory chatClientFactory,
        IAINewsService aiNewsService,
        IWinFormNewsService winFormNewsService,
        IVideoService videoService,
        IOptions<MistralOptions> options,
        ILogger<AiSummarizationService> logger)
    {
        _blogAggregationService = blogAggregationService;
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
        _chatClientFactory = chatClientFactory;
        _aiNewsService = aiNewsService;
        _winFormNewsService = winFormNewsService;
        _videoService = videoService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<AiContentSummary>> GetLatestBlogSummariesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        List<BaseNews> globalPosts = new List<BaseNews>();
        var posts = (await _blogAggregationService.GetLatestPostsAsync()).Where(w=>w.PublishedDate >= DateTime.Today.AddDays(-1)).ToList();
        var aiNewsTask = (await _aiNewsService.GetLatestAINewsAsync()).Where(w => w.PublishedDate >= DateTime.Today.AddDays(-1)).ToList();
        var winFormTask = (await _winFormNewsService.GetLatestWinFormNewsAsync()).Where(w => w.PublishedDate >= DateTime.Today.AddDays(-1)).ToList();
        //var videoTask = (await _videoService.GetLatestVideosAsync()).Where(w => w.PublishedDate >= DateTime.Today.AddDays(-1)).ToList(); TO be implemented after

        if (posts.Count == 0 && aiNewsTask.Count == 0 && winFormTask.Count == 0)
        {
            return new List<AiContentSummary>();
        }

        globalPosts.AddRange(posts);
        globalPosts.AddRange(aiNewsTask);
        globalPosts.AddRange(winFormTask);
        //posts.AddRange(videoTask); TO be implemented after

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

        using var gate = new SemaphoreSlim(3);
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

        return (await Task.WhenAll(tasks)).Where(s => s is not null).Select(s => s!).ToList();
    }

    private async Task<AiContentSummary?> SummarizePostAsync(IChatClient chatClient, BaseNews post, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(post.Url))
        {
            return null;
        }

        var cacheKey = GetCacheKey(post.Url);
        var cached = _cacheService.Get<AiContentSummary>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VeilleNet-AI-Summary");

        string html;
        try
        {
            html = await httpClient.GetStringAsync(post.Url, cancellationToken);
        }
        catch
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
    Your goal is to produce a concise, expert-level summary of each article.\n\n
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
            new(ChatRole.User, $"Titre: {post.Title}\nSource: {post.Source}\nURL: {post.Url}\n\nContenu (extrait):\n{content}\n\nTâche: Résume en 4-6 puces, puis une phrase 'Pourquoi c'est important'.")
        };

        var chatOptions = new ChatOptions
        {
            Temperature = _options.Temperature
            // MaxTokens is not available in ChatOptions, will be handled by the implementation
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

        string htmlReponseText = AddHtmlToText(responseText);

        var result = new AiContentSummary
        {
            Title = post.Title,
            Url = post.Url,
            Source = post.Source,
            PublishedDate = post.PublishedDate,
            Summary = htmlReponseText
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

    private static string GetCacheKey(string url)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return "AiSummary:" + Convert.ToHexString(hash);
    }
}
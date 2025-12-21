using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;
using VeilleNet.Services.News;
using VeilleNet.Services.Tools;
using VeilleNet.Services.Data;
using System.Security.Cryptography;
using System.Text;

namespace VeilleNet.Pages;

public class IndexModel : PageModel
{
    private readonly IBlogAggregationService _blogService;
    private readonly IGitHubService _gitHubService;
    private readonly IReleaseNewsService _releaseService;
    private readonly IAINewsService _aiNewsService;
    private readonly IWinFormNewsService _winFormNewsService;
    private readonly IVideoService _videoService;
    private readonly IStackOverflowService _stackOverflowService;
    private readonly ICacheService _cacheService;
    private readonly INewsletterService _newsletterService;
    private readonly INewsRepository _newsRepository;
    private readonly ILogger<IndexModel> _logger;

    public List<BaseNews> BlogPosts { get; set; } = new();
    public List<GitHubProject> TrendingProjects { get; set; } = new();
    public List<ReleaseNews> ReleaseNews { get; set; } = new();
    public List<BaseNews> AINews { get; set; } = new();
    public List<BaseNews> WinFormNews { get; set; } = new();
    public List<Video> Videos { get; set; } = new();
    public List<StackOverflowQuestion> StackOverflowQuestions { get; set; } = new();
    
    // Dictionary to store AI summaries by URL
    private Dictionary<string, AiContentSummary> _aiSummaries = new();

    public IndexModel(
        IBlogAggregationService blogService,
        IGitHubService gitHubService,
        IReleaseNewsService releaseService,
        IAINewsService aiNewsService,
        IWinFormNewsService winFormNewsService,
        IVideoService videoService,
        IStackOverflowService stackOverflowService,
        ICacheService cacheService,
        INewsletterService newsletterService,
        INewsRepository newsRepository,
        ILogger<IndexModel> logger)
    {
        _blogService = blogService;
        _gitHubService = gitHubService;
        _releaseService = releaseService;
        _aiNewsService = aiNewsService;
        _winFormNewsService = winFormNewsService;
        _videoService = videoService;
        _stackOverflowService = stackOverflowService;
        _cacheService = cacheService;
        _newsletterService = newsletterService;
        _newsRepository = newsRepository;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        // Load all dashboard data in parallel
        var blogTask = _blogService.GetLatestPostsAsync();
        var githubTask = _gitHubService.GetTrendingCSharpProjectsAsync();
        var releaseTask = _releaseService.GetLatestReleasesAsync();
        var aiNewsTask = _aiNewsService.GetLatestAINewsAsync();
        var winFormTask = _winFormNewsService.GetLatestWinFormNewsAsync();
        var videoTask = _videoService.GetLatestVideosAsync();
        var stackOverflowTask = _stackOverflowService.GetLatestQuestionsAsync();

        await Task.WhenAll(blogTask, githubTask, releaseTask, aiNewsTask, winFormTask, videoTask, stackOverflowTask);

        BlogPosts = await blogTask;
        TrendingProjects = await githubTask;
        ReleaseNews = await releaseTask;
        AINews = await aiNewsTask;
        WinFormNews = await winFormTask;
        Videos = await videoTask;
        StackOverflowQuestions = await stackOverflowTask;
        
        // Load AI summaries from database for all news items
        await LoadAiSummariesAsync();
    }

    private async Task LoadAiSummariesAsync()
    {
        try
        {
            // Get all URLs from displayed news
            var allUrls = new List<string>();
            allUrls.AddRange(BlogPosts.Select(p => p.Url));
            allUrls.AddRange(AINews.Select(p => p.Url));
            allUrls.AddRange(WinFormNews.Select(p => p.Url));
            
            // Get AI summaries for all URLs in one query
            foreach (var url in allUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                try
                {
                    var summary = await _newsRepository.GetAiSummaryByUrlAsync(url);
                    if (summary != null)
                    {
                        _aiSummaries[url] = summary.ToAiContentSummary();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error loading AI summary for URL: {Url}", url);
                }
            }
            
            _logger.LogInformation("Loaded {Count} AI summaries from database", _aiSummaries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading AI summaries");
        }
    }

    public async Task<IActionResult> OnPostQuickSubscribeAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ViewData["NewsletterMessage"] = "Please enter a valid email address.";
            ViewData["NewsletterMessageType"] = "error";
            await OnGetAsync();
            return Page();
        }

        try
        {
            var isAlreadySubscribed = await _newsletterService.IsSubscribedAsync(email);
            
            if (isAlreadySubscribed)
            {
                ViewData["NewsletterMessage"] = "You're already subscribed! 🎉";
                ViewData["NewsletterMessageType"] = "info";
                _logger.LogInformation("Quick subscribe attempt for already subscribed email: {Email}", email);
            }
            else
            {
                var success = await _newsletterService.SubscribeAsync(email, "Homepage Quick Subscribe");
                
                if (success)
                {
                    ViewData["NewsletterMessage"] = "Success! Check your inbox tomorrow for your daily .NET updates! 🎊";
                    ViewData["NewsletterMessageType"] = "success";
                    ViewData["TriggerFireworks"] = "true"; // Flag pour l'animation
                    _logger.LogInformation("New subscriber via homepage: {Email}", email);
                }
                else
                {
                    ViewData["NewsletterMessage"] = "Oops! Something went wrong. Please try again.";
                    ViewData["NewsletterMessageType"] = "error";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during quick newsletter subscription for {Email}", email);
            ViewData["NewsletterMessage"] = "An error occurred. Please try again later.";
            ViewData["NewsletterMessageType"] = "error";
        }

        await OnGetAsync();
        return Page();
    }

    public bool HasAiSummary(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return _aiSummaries.ContainsKey(url);
    }
    
    public string GetAiSummary(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || !_aiSummaries.ContainsKey(url))
        {
            return string.Empty;
        }
        
        return _aiSummaries[url].Summary;
    }

    private static string GetCacheKey(string url)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return "AiSummary:" + Convert.ToHexString(hash);
    }
}

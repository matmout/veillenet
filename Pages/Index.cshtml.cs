using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using VeilleNet.Models;
using VeilleNet.Services;
using VeilleNet.Services.News;
using VeilleNet.Services.Tools;

namespace VeilleNet.Pages;

public class IndexModel : PageModel
{
    private readonly IBlogAggregationService _blogService;
    private readonly IGitHubService _gitHubService;
    private readonly IAINewsService _aiNewsService;
    private readonly IWinFormNewsService _winFormNewsService;
    private readonly IVideoService _videoService;
    private readonly IStackOverflowService _stackOverflowService;
    private readonly IXPostsService _xPostsService;
    private readonly INewsletterService _newsletterService;
    private readonly ILogger<IndexModel> _logger;

    public List<BaseNews> BlogPosts { get; set; } = new();
    public List<GitHubProject> TrendingProjects { get; set; } = new();
    public List<BaseNews> AINews { get; set; } = new();
    public List<BaseNews> WinFormNews { get; set; } = new();
    public List<Video> Videos { get; set; } = new();
    public List<StackOverflowQuestion> StackOverflowQuestions { get; set; } = new();
    public List<XPost> OfficialXPosts { get; set; } = new();

    public int ActiveNewsletterSubscribersCount { get; private set; }
    public bool IsXPostsFeatureEnabled { get; }

    public IndexModel(
        IBlogAggregationService blogService,
        IGitHubService gitHubService,
        IAINewsService aiNewsService,
        IWinFormNewsService winFormNewsService,
        IVideoService videoService,
        IStackOverflowService stackOverflowService,
        IXPostsService xPostsService,
        IOptions<XApiOptions> xApiOptions,
        INewsletterService newsletterService,
        ILogger<IndexModel> logger)
    {
        _blogService = blogService;
        _gitHubService = gitHubService;
        _aiNewsService = aiNewsService;
        _winFormNewsService = winFormNewsService;
        _videoService = videoService;
        _stackOverflowService = stackOverflowService;
        _xPostsService = xPostsService;
        IsXPostsFeatureEnabled = xApiOptions.Value.Enabled;
        _newsletterService = newsletterService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        // Load all dashboard data in parallel
        var blogTask = _blogService.GetLatestPostsAsync();
        var githubTask = _gitHubService.GetTrendingCSharpProjectsAsync();
        var aiNewsTask = _aiNewsService.GetLatestAINewsAsync();
        var winFormTask = _winFormNewsService.GetLatestWinFormNewsAsync();
        var videoTask = _videoService.GetLatestVideosAsync();
        var stackOverflowTask = _stackOverflowService.GetLatestQuestionsAsync();
        var xPostsTask = IsXPostsFeatureEnabled
            ? _xPostsService.GetLatestOfficialPostsAsync()
            : Task.FromResult(new List<XPost>());
        var subscribersCountTask = _newsletterService.GetActiveSubscribersCountAsync();

        await Task.WhenAll(blogTask, githubTask, aiNewsTask, winFormTask, videoTask, stackOverflowTask, xPostsTask, subscribersCountTask);

        BlogPosts = await blogTask;
        TrendingProjects = await githubTask;
        AINews = await aiNewsTask;
        WinFormNews = await winFormTask;
        Videos = await videoTask;
        StackOverflowQuestions = await stackOverflowTask;
        OfficialXPosts = await xPostsTask;
        ActiveNewsletterSubscribersCount = await subscribersCountTask;
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
                    ViewData["NewsletterMessage"] = "A confirmation message has been sent to you. Please check your email to complete your subscription. 📧";
                    ViewData["NewsletterMessageType"] = "success";
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
}

using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;

namespace VeilleNet.Pages;

public class IndexModel : PageModel
{
    private readonly IBlogAggregationService _blogService;
    private readonly IGitHubService _gitHubService;
    private readonly IReleaseNewsService _releaseService;
    private readonly IAINewsService _aiNewsService;
    private readonly IWinFormNewsService _winFormNewsService;
    private readonly IVideoService _videoService;
    private readonly ILLMService _llmService;

    public List<BlogPost> BlogPosts { get; set; } = new();
    public List<GitHubProject> TrendingProjects { get; set; } = new();
    public List<ReleaseNews> ReleaseNews { get; set; } = new();
    public List<AINews> AINews { get; set; } = new();
    public List<WinFormNews> WinFormNews { get; set; } = new();
    public List<Video> Videos { get; set; } = new();
    public List<LLM> LatestLLMs { get; set; } = new();

    public IndexModel(
        IBlogAggregationService blogService,
        IGitHubService gitHubService,
        IReleaseNewsService releaseService,
        IAINewsService aiNewsService,
        IWinFormNewsService winFormNewsService,
        IVideoService videoService,
        ILLMService llmService)
    {
        _blogService = blogService;
        _gitHubService = gitHubService;
        _releaseService = releaseService;
        _aiNewsService = aiNewsService;
        _winFormNewsService = winFormNewsService;
        _videoService = videoService;
        _llmService = llmService;
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
        var llmTask = _llmService.GetTopLLMsAsync(10);

        await Task.WhenAll(blogTask, githubTask, releaseTask, aiNewsTask, winFormTask, videoTask, llmTask);

        BlogPosts = await blogTask;
        TrendingProjects = await githubTask;
        ReleaseNews = await releaseTask;
        AINews = await aiNewsTask;
        WinFormNews = await winFormTask;
        Videos = await videoTask;
        LatestLLMs = await llmTask;
    }
}

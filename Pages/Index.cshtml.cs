using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;
using VeilleNet.Services.News;
using VeilleNet.Services.Tools;
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
    private readonly ICacheService _cacheService;

    public List<BaseNews> BlogPosts { get; set; } = new();
    public List<GitHubProject> TrendingProjects { get; set; } = new();
    public List<ReleaseNews> ReleaseNews { get; set; } = new();
    public List<BaseNews> AINews { get; set; } = new();
    public List<BaseNews> WinFormNews { get; set; } = new();
    public List<Video> Videos { get; set; } = new();

    public IndexModel(
        IBlogAggregationService blogService,
        IGitHubService gitHubService,
        IReleaseNewsService releaseService,
        IAINewsService aiNewsService,
        IWinFormNewsService winFormNewsService,
        IVideoService videoService,
        ICacheService cacheService)
    {
        _blogService = blogService;
        _gitHubService = gitHubService;
        _releaseService = releaseService;
        _aiNewsService = aiNewsService;
        _winFormNewsService = winFormNewsService;
        _videoService = videoService;
        _cacheService = cacheService;
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

        await Task.WhenAll(blogTask, githubTask, releaseTask, aiNewsTask, winFormTask, videoTask);

        BlogPosts = await blogTask;
        TrendingProjects = await githubTask;
        ReleaseNews = await releaseTask;
        AINews = await aiNewsTask;
        WinFormNews = await winFormTask;
        Videos = await videoTask;
    }

    public bool HasAiSummary(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        var cacheKey = GetCacheKey(url);
        var cached = _cacheService.Get<AiContentSummary>(cacheKey);
        return cached != null;
    }

    private static string GetCacheKey(string url)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return "AiSummary:" + Convert.ToHexString(hash);
    }
}

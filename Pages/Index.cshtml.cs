using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;

namespace VeilleNet.Pages;

public class IndexModel : PageModel
{
    private readonly IBlogAggregationService _blogService;
    private readonly IGitHubService _gitHubService;
    private readonly IReleaseNewsService _releaseService;

    public List<BlogPost> BlogPosts { get; set; } = new();
    public List<GitHubProject> TrendingProjects { get; set; } = new();
    public List<ReleaseNews> ReleaseNews { get; set; } = new();

    public IndexModel(
        IBlogAggregationService blogService,
        IGitHubService gitHubService,
        IReleaseNewsService releaseService)
    {
        _blogService = blogService;
        _gitHubService = gitHubService;
        _releaseService = releaseService;
    }

    public async Task OnGetAsync()
    {
        // Load all dashboard data in parallel
        var blogTask = _blogService.GetLatestPostsAsync();
        var githubTask = _gitHubService.GetTrendingCSharpProjectsAsync();
        var releaseTask = _releaseService.GetLatestReleasesAsync();

        await Task.WhenAll(blogTask, githubTask, releaseTask);

        BlogPosts = await blogTask;
        TrendingProjects = await githubTask;
        ReleaseNews = await releaseTask;
    }
}

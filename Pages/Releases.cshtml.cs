using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services.News;

namespace VeilleNet.Pages;

public class ReleasesModel : PageModel
{
    private readonly IReleaseNewsService _releaseNewsService;

    public ReleasesModel(IReleaseNewsService releaseNewsService)
    {
        _releaseNewsService = releaseNewsService;
    }

    public List<ReleaseNews> Releases { get; set; } = [];

    /// <summary>Distinct framework names for the filter buttons.</summary>
    public List<string> Frameworks { get; set; } = [];

    /// <summary>Currently selected framework filter (null = all).</summary>
    public string? SelectedFramework { get; set; }

    public async Task OnGetAsync(string? framework)
    {
        SelectedFramework = framework;
        var all = await _releaseNewsService.GetLatestReleasesAsync();

        Frameworks = all
            .Select(r => ExtractFramework(r.Title))
            .Distinct()
            .Order()
            .ToList();

        Releases = string.IsNullOrEmpty(framework)
            ? all
            : all.Where(r => ExtractFramework(r.Title) == framework).ToList();
    }

    /// <summary>Extract framework prefix from title (e.g. ".NET Runtime — v10.0.0" → ".NET Runtime").</summary>
    private static string ExtractFramework(string title)
    {
        var dashIndex = title.IndexOf('—');
        return dashIndex > 0 ? title[..dashIndex].Trim() : title;
    }
}

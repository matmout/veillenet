using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services.News;

namespace VeilleNet.Pages;

public class HistoryModel : PageModel
{
    private static readonly TimeSpan DefaultLookback = TimeSpan.FromDays(30);
    private readonly INewsHistoryService _newsHistoryService;


    public HistoryModel(INewsHistoryService newsHistoryService)
    {
        _newsHistoryService = newsHistoryService;
    }

    public List<BaseNews> InitialNews { get; private set; } = new();
    public List<string> Sources { get; private set; } = new();
    public int TotalCount { get; private set; }

    public async Task OnGetAsync()
    {
        var defaultStart = DateTime.UtcNow.Subtract(DefaultLookback);
        ViewData["DefaultStartDate"] = defaultStart.ToString("yyyy-MM-dd");

        var result = await _newsHistoryService.SearchAsync(null, defaultStart, null, null, 1, 20);
        InitialNews = result.Items;
        TotalCount = result.TotalCount;
        Sources = await _newsHistoryService.GetSourcesAsync();


    }

    public async Task<IActionResult> OnGetSearchAsync(string? keyword, DateTime? startDate, DateTime? endDate, string? source, int page = 1, int pageSize = 20)
    {
        var effectiveStart = startDate ?? DateTime.UtcNow.Subtract(DefaultLookback);

        var result = await _newsHistoryService.SearchAsync(keyword, effectiveStart, endDate, source, page, pageSize);

        var items = result.Items.Select(n => new
        {
            title = n.Title,
            url = n.Url,
            summary = n.Summary,
            publishedDate = n.PublishedDate.ToString("dd MMM yyyy"),
            source = n.Source,
            image = string.IsNullOrWhiteSpace(n.Image) ? Url.Content("~/images/newsia.png") : n.Image,
            hasSummary = n.HasAiSummary
        });

        return new JsonResult(new { items, total = result.TotalCount });
    }

}

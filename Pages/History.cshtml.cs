using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services.News;
using VeilleNet.Services.Data;

public class HistoryModel : PageModel
{
    private static readonly TimeSpan DefaultLookback = TimeSpan.FromDays(30);
    private readonly INewsHistoryService _newsHistoryService;
    private readonly INewsRepository _newsRepository;
    private readonly HashSet<string> _aiSummaryUrls = new();

    public HistoryModel(INewsHistoryService newsHistoryService, INewsRepository newsRepository)
    {
        _newsHistoryService = newsHistoryService;
        _newsRepository = newsRepository;
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

        await LoadAiSummariesAsync(InitialNews.Select(n => n.Url));
    }

    public async Task<IActionResult> OnGetSearchAsync(string? keyword, DateTime? startDate, DateTime? endDate, string? source, int page = 1, int pageSize = 20)
    {
        var effectiveStart = startDate ?? DateTime.UtcNow.Subtract(DefaultLookback);
        var result = await _newsHistoryService.SearchAsync(keyword, effectiveStart, endDate, source, page, pageSize);
        var urls = result.Items.Select(n => n.Url).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();

        var hasSummary = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var url in urls)
        {
            var summary = await _newsRepository.GetAiSummaryByUrlAsync(url);
            if (summary != null)
            {
                hasSummary.Add(url);
            }
        }

        var items = result.Items.Select(n => new
        {
            title = n.Title,
            url = n.Url,
            summary = n.Summary,
            publishedDate = n.PublishedDate.ToString("dd MMM yyyy"),
            source = n.Source,
            image = string.IsNullOrWhiteSpace(n.Image) ? Url.Content("~/images/newsia.png") : n.Image,
            hasSummary = hasSummary.Contains(n.Url)
        });

        return new JsonResult(new { items, total = result.TotalCount });
    }

    public bool HasAiSummary(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return _aiSummaryUrls.Contains(url);
    }

    private async Task LoadAiSummariesAsync(IEnumerable<string?> urls)
    {
        foreach (var url in urls.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct())
        {
            var summary = await _newsRepository.GetAiSummaryByUrlAsync(url!);
            if (summary != null)
            {
                _aiSummaryUrls.Add(url!);
            }
        }
    }
}

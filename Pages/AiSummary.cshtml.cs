using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services.Agent;

namespace VeilleNet.Pages;

public class AiSummaryModel : PageModel
{
    private readonly IAiSummarizationService _aiSummarizationService;

    public List<AiContentSummary> Summaries { get; private set; } = new();

    public AiSummaryModel(IAiSummarizationService aiSummarizationService)
    {
        _aiSummarizationService = aiSummarizationService;
    }

    public async Task OnGetAsync()
    {
        Summaries = await _aiSummarizationService.GetLatestBlogSummariesAsync(12);
    }
}

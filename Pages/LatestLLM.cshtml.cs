using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;

namespace VeilleNet.Pages;

public class LatestLLMModel : PageModel
{
    private readonly ILLMService _llmService;

    public List<LLM> LLMs { get; set; } = new();

    public LatestLLMModel(ILLMService llmService)
    {
        _llmService = llmService;
    }

    public async Task OnGetAsync()
    {
        LLMs = await _llmService.GetLatestLLMsAsync();
    }
}

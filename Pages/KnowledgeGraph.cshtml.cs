using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Services.Data;

namespace VeilleNet.Pages;

public class KnowledgeGraphModel : PageModel
{
    private readonly INewsRepository _newsRepository;

    public KnowledgeGraphModel(INewsRepository newsRepository)
    {
        _newsRepository = newsRepository;
    }

    public void OnGet()
    {
    }
}

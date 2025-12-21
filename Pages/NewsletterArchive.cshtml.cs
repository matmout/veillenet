using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models.Entities;
using VeilleNet.Services.Data;

namespace VeilleNet.Pages;

public class NewsletterArchiveModel : PageModel
{
    private readonly INewsRepository _newsRepository;

    public NewsletterArchiveModel(INewsRepository newsRepository)
    {
        _newsRepository = newsRepository;
    }

    public List<DailyNewsletter> Newsletters { get; set; } = new();

    public async Task OnGetAsync()
    {
        Newsletters = await _newsRepository.GetRecentNewslettersAsync(30);
    }
}

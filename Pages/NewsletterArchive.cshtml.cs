using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models.Entities;
using VeilleNet.Services.Data;

namespace VeilleNet.Pages;

public class NewsletterArchiveModel : PageModel
{
    private readonly INewsletterRepository _newsletterRepository;

    public NewsletterArchiveModel(INewsletterRepository newsletterRepository)
    {
        _newsletterRepository = newsletterRepository;
    }

    public List<DailyNewsletter> Newsletters { get; set; } = new();

    public async Task OnGetAsync()
    {
        Newsletters = await _newsletterRepository.GetRecentNewslettersAsync(30);
    }
}

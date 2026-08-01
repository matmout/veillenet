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

    public int SentCount => Newsletters.Count(n => n.IsSent);
    public int PendingCount => Newsletters.Count(n => !n.IsSent);

    public async Task OnGetAsync()
    {
        Newsletters = await _newsletterRepository.GetRecentNewslettersAsync(30);
    }

    public static string ShortHash(DailyNewsletter newsletter)
    {
        unchecked
        {
            int seed = newsletter.Id == 0
                ? newsletter.NewsletterDate.DayNumber * 31 + newsletter.Subject.GetHashCode()
                : (int)(newsletter.Id * 2654435761u);
            uint mixed = (uint)(seed ^ (seed >> 16));
            return mixed.ToString("x8")[..7];
        }
    }
}

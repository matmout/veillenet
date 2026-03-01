using VeilleNet.Models.Entities;

namespace VeilleNet.Services.Data;

/// <summary>
/// Repository for daily newsletter management and sending.
/// </summary>
public interface INewsletterRepository
{
    Task<DailyNewsletter?> GetNewsletterByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<DailyNewsletter?> GetTodayNewsletterAsync(CancellationToken cancellationToken = default);
    Task<List<DailyNewsletter>> GetRecentNewslettersAsync(int count = 30, CancellationToken cancellationToken = default);
    Task<DailyNewsletter> CreateOrUpdateNewsletterAsync(DailyNewsletter newsletter, CancellationToken cancellationToken = default);
    Task MarkNewsletterAsSentAsync(DateOnly date, int recipientCount, CancellationToken cancellationToken = default);
    Task<bool> HasNewsletterForDateAsync(DateOnly date, CancellationToken cancellationToken = default);
}

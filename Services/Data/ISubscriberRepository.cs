using VeilleNet.Models.Entities;

namespace VeilleNet.Services.Data;

/// <summary>
/// Repository for newsletter subscriber management.
/// </summary>
public interface ISubscriberRepository
{
    Task<NewsletterSubscriber?> GetSubscriberByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<NewsletterSubscriber>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default);
    Task<List<NewsletterSubscriber>> GetAllSubscribersAsync(CancellationToken cancellationToken = default);
    Task<NewsletterSubscriber> SubscribeAsync(string email, string source = "Website", bool isActive = true, CancellationToken cancellationToken = default);
    Task<NewsletterSubscriber> UnsubscribeAsync(string email, string? reason = null, CancellationToken cancellationToken = default);
    Task<bool> IsSubscribedAsync(string email, CancellationToken cancellationToken = default);
    Task IncrementEmailSentAsync(string email, CancellationToken cancellationToken = default);
    Task<int> GetActiveSubscribersCountAsync(CancellationToken cancellationToken = default);
    Task<string> GenerateUnsubscribeTokenAsync(string email, CancellationToken cancellationToken = default);
    Task<string> GenerateConfirmationTokenAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> HasValidUnsubscribeTokenAsync(string email, CancellationToken cancellationToken = default);
    Task<NewsletterSubscriber?> GetSubscriberByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<NewsletterSubscriber?> GetSubscriberByConfirmationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> UnsubscribeWithTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> ConfirmSubscriptionAsync(string token, CancellationToken cancellationToken = default);
}

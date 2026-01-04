using VeilleNet.Models;
using VeilleNet.Services.Data;

namespace VeilleNet.Services.Tools;

public interface INewsletterService
{
    Task<bool> SubscribeAsync(string email, string source = "Website");
    Task<bool> UnsubscribeAsync(string email, string? reason = null);
    Task<bool> IsSubscribedAsync(string email);
    Task<List<string>> GetAllActiveSubscribersEmailsAsync();
    Task<int> GetActiveSubscribersCountAsync();
    Task IncrementEmailSentAsync(string email);
}

public class NewsletterService : INewsletterService
{
    private readonly INewsRepository _newsRepository;
    private readonly ILogger<NewsletterService> _logger;
    private readonly IEmailService _emailService;

    public NewsletterService(INewsRepository newsRepository, ILogger<NewsletterService> logger, IEmailService emailService)
    {
        _newsRepository = newsRepository;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<bool> SubscribeAsync(string email, string source = "Website")
    {
        try
        {
            await _newsRepository.SubscribeAsync(email, source);
            _logger.LogInformation("Subscriber added: {Email} from {Source}", email, source);
            
            // Send notification email to reporting address
            await _emailService.SendSubscriptionNotificationEmailAsync(email, source);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing email: {Email}", email);
            return false;
        }
    }

    public async Task<bool> UnsubscribeAsync(string email, string? reason = null)
    {
        try
        {
            await _newsRepository.UnsubscribeAsync(email, reason);
            _logger.LogInformation("Subscriber unsubscribed: {Email}, Reason: {Reason}", email, reason ?? "Not specified");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing email: {Email}", email);
            return false;
        }
    }

    public async Task<bool> IsSubscribedAsync(string email)
    {
        return await _newsRepository.IsSubscribedAsync(email);
    }

    public async Task<List<string>> GetAllActiveSubscribersEmailsAsync()
    {
        var subscribers = await _newsRepository.GetActiveSubscribersAsync();
        return subscribers.Select(s => s.Email).ToList();
    }

    public async Task<int> GetActiveSubscribersCountAsync()
    {
        return await _newsRepository.GetActiveSubscribersCountAsync();
    }

    public async Task IncrementEmailSentAsync(string email)
    {
        await _newsRepository.IncrementEmailSentAsync(email);
    }
}

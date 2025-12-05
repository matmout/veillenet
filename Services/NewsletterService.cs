using VeilleNet.Models;

namespace VeilleNet.Services;

public interface INewsletterService
{
    Task<bool> SubscribeAsync(NewsletterSubscription subscription);
    Task<List<NewsletterSubscription>> GetAllSubscriptionsAsync();
}

public class NewsletterService : INewsletterService
{
    private readonly ICacheService _cacheService;
    private const string CacheKey = "NewsletterSubscriptions";

    public NewsletterService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<bool> SubscribeAsync(NewsletterSubscription subscription)
    {
        var subscriptions = await GetAllSubscriptionsAsync();
        
        // Check if email already exists
        if (subscriptions.Any(s => s.Email.Equals(subscription.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        subscription.SubscribedAt = DateTime.UtcNow;
        subscriptions.Add(subscription);
        
        // Store indefinitely (or until cache is cleared)
        _cacheService.Set(CacheKey, subscriptions, TimeSpan.FromDays(365));
        
        return true;
    }

    public async Task<List<NewsletterSubscription>> GetAllSubscriptionsAsync()
    {
        var subscriptions = _cacheService.Get<List<NewsletterSubscription>>(CacheKey);
        return await Task.FromResult(subscriptions ?? new List<NewsletterSubscription>());
    }
}

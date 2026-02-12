using Microsoft.EntityFrameworkCore;
using VeilleNet.Data;
using VeilleNet.Models;
using VeilleNet.Models.Entities;

namespace VeilleNet.Services.Data;

public class NewsRepository : INewsRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<NewsRepository> _logger;

    public NewsRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger<NewsRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database operation failed: {OperationName}", operationName);
            throw;
        }
    }

    private async Task<T> ExecuteWithContextAsync<T>(Func<ApplicationDbContext, Task<T>> operation, string operationName, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            return await operation(context);
        }, operationName);
    }

    /// <summary>
    /// Converts DateTime to UTC. If Kind is Unspecified, assumes Paris time (Europe/Paris).
    /// </summary>
    private static DateTime EnsureUtc(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
        {
            return dateTime;
        }

        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            // Assume Paris time (UTC+1 or UTC+2 depending on DST)
            var parisTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time"); // Windows
            try
            {
                return TimeZoneInfo.ConvertTimeToUtc(dateTime, parisTimeZone);
            }
            catch
            {
                // Fallback: treat as UTC if conversion fails
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
        }

        // DateTimeKind.Local
        return dateTime.ToUniversalTime();
    }

    // News Articles
    public async Task<NewsArticle?> GetNewsArticleByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NewsArticles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(n => n.Url == url, cancellationToken),
            nameof(GetNewsArticleByUrlAsync), cancellationToken);
    }

    public async Task<List<NewsArticle>> GetRecentNewsArticlesAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NewsArticles
                    .AsNoTracking()
                    .OrderByDescending(n => n.PublishedDate)
                    .Take(count)
                    .ToListAsync(cancellationToken),
            nameof(GetRecentNewsArticlesAsync), cancellationToken);
    }

    public async Task<NewsArticle> AddNewsArticleAsync(NewsArticle article, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            // Ensure all DateTime values are UTC
            article.PublishedDate = EnsureUtc(article.PublishedDate);
            article.CreatedAt = EnsureUtc(article.CreatedAt);
            article.UpdatedAt = EnsureUtc(article.UpdatedAt);

            context.NewsArticles.Add(article);
            await context.SaveChangesAsync(cancellationToken);
            return article;
        }, nameof(AddNewsArticleAsync), cancellationToken);
    }

    public async Task<NewsArticle> UpdateNewsArticleAsync(NewsArticle article, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            // Ensure all DateTime values are UTC
            article.PublishedDate = EnsureUtc(article.PublishedDate);
            article.UpdatedAt = EnsureUtc(DateTime.UtcNow);

            context.Entry(article).State = EntityState.Modified;
            await context.SaveChangesAsync(cancellationToken);
            return article;
        }, nameof(UpdateNewsArticleAsync), cancellationToken);
    }

    public async Task<List<NewsArticle>> AddOrUpdateNewsArticlesAsync(List<BaseNews> news, CancellationToken cancellationToken = default)
    {
        if (!news.Any())
        {
            return new List<NewsArticle>();
        }

        return await ExecuteWithContextAsync(async context =>
        {
            var results = new List<NewsArticle>();
            
            // Get all URLs to check in one query
            var urls = news.Select(n => n.Url).ToList();
            var existingArticles = await context.NewsArticles
                .Where(n => urls.Contains(n.Url))
                .ToListAsync(cancellationToken);

            var existingUrls = existingArticles.ToDictionary(a => a.Url, a => a);

            foreach (var item in news)
            {
                if (existingUrls.TryGetValue(item.Url, out var existing))
                {
                    // Update existing
                    existing.Title = item.Title;
                    existing.Summary = item.Summary;
                    existing.Author = item.Author;
                    existing.Source = item.Source;
                    existing.Category = item.Category;
                    existing.Image = item.Image;
                    existing.PublishedDate = EnsureUtc(item.PublishedDate);
                    existing.UpdatedAt = DateTime.UtcNow;
                    
                    results.Add(existing);
                }
                else
                {
                    // Add new
                    var newArticle = NewsArticle.FromBaseNews(item);
                    newArticle.PublishedDate = EnsureUtc(newArticle.PublishedDate);
                    newArticle.CreatedAt = DateTime.UtcNow;
                    newArticle.UpdatedAt = DateTime.UtcNow;

                    context.NewsArticles.Add(newArticle);
                    results.Add(newArticle);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            return results;
        }, nameof(AddOrUpdateNewsArticlesAsync), cancellationToken);
    }

    public async Task<(List<NewsArticle> Items, int TotalCount)> SearchNewsArticlesAsync(string? keyword, DateTime? startDate, DateTime? endDate, string? source, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var query = context.NewsArticles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var pattern = $"%{keyword}%";
                query = query.Where(n => EF.Functions.ILike(n.Title, pattern) || EF.Functions.ILike(n.Summary, pattern));
            }

            if (startDate.HasValue)
            {
                var startUtc = EnsureUtc(startDate.Value);
                query = query.Where(n => n.PublishedDate.Date >= startUtc);
            }

            if (endDate.HasValue)
            {
                var endOfDayLocal = endDate.Value.Date.AddDays(1);
                var endUtc = EnsureUtc(endOfDayLocal);
                query = query.Where(n => n.PublishedDate.Date <= endUtc);
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                query = query.Where(n => n.Source == source);
            }

            take = Math.Clamp(take, 1, 200);
            skip = Math.Max(0, skip);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(n => n.PublishedDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }, nameof(SearchNewsArticlesAsync), cancellationToken);
    }

    public async Task<List<string>> GetAllNewsSourcesAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NewsArticles
                    .AsNoTracking()
                    .Select(n => n.Source)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToListAsync(cancellationToken),
            nameof(GetAllNewsSourcesAsync), cancellationToken);
    }

    // AI Summaries
    public async Task<AiSummaryEntity?> GetAiSummaryByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.AiSummaries
                    .FirstOrDefaultAsync(s => s.Url == url, cancellationToken),
            nameof(GetAiSummaryByUrlAsync), cancellationToken);
    }

    public async Task<List<AiSummaryEntity>> GetRecentAiSummariesAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.AiSummaries
                    .AsNoTracking()
                    .OrderByDescending(s => s.SummaryDate)
                    .Take(count)
                    .ToListAsync(cancellationToken),
            nameof(GetRecentAiSummariesAsync), cancellationToken);
    }

    public async Task<List<AiSummaryEntity>> GetAiSummariesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.AiSummaries
                    .AsNoTracking()
                    .Where(s => s.SummaryDate >= startDate && s.SummaryDate <= endDate)
                    .OrderByDescending(s => s.SummaryDate)
                    .ToListAsync(cancellationToken),
            nameof(GetAiSummariesByDateRangeAsync), cancellationToken);
    }

    public async Task<AiSummaryEntity> AddAiSummaryAsync(AiSummaryEntity summary, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            // Ensure all DateTime values are UTC
            summary.PublishedDate = EnsureUtc(summary.PublishedDate);
            summary.SummaryDate = EnsureUtc(summary.SummaryDate);
            summary.CreatedAt = DateTime.UtcNow;
            summary.UpdatedAt = DateTime.UtcNow;

            context.AiSummaries.Add(summary);
            await context.SaveChangesAsync(cancellationToken);
            return summary;
        }, nameof(AddAiSummaryAsync), cancellationToken);
    }

    public async Task<AiSummaryEntity> UpdateAiSummaryAsync(AiSummaryEntity summary, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            // Ensure all DateTime values are UTC
            summary.PublishedDate = EnsureUtc(summary.PublishedDate);
            summary.SummaryDate = EnsureUtc(summary.SummaryDate);
            summary.UpdatedAt = DateTime.UtcNow;

            context.Entry(summary).State = EntityState.Modified;
            await context.SaveChangesAsync(cancellationToken);
            return summary;
        }, nameof(UpdateAiSummaryAsync), cancellationToken);
    }

    public async Task<AiSummaryEntity> AddOrUpdateAiSummaryAsync(AiContentSummary summary, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var existing = await context.AiSummaries
                .FirstOrDefaultAsync(s => s.Url == summary.Url, cancellationToken);

            // Get the corresponding news article
            var newsArticle = await context.NewsArticles
                .FirstOrDefaultAsync(n => n.Url == summary.Url, cancellationToken);

            int? newsArticleId = newsArticle?.Id;

            if (existing != null)
            {
                // Update existing
                existing.Title = summary.Title;
                existing.Source = summary.Source;
                existing.PublishedDate = EnsureUtc(summary.PublishedDate);
                existing.Summary = summary.Summary;
                existing.AiGenerated = summary.AiGenerated;
                existing.SummaryDate = EnsureUtc(summary.SummaryDate);
                existing.UpdatedAt = DateTime.UtcNow;
                existing.NewsArticleId = newsArticleId; // Link to news article

                await context.SaveChangesAsync(cancellationToken);
                return existing;
            }
            else
            {
                // Add new
                var newSummary = AiSummaryEntity.FromAiContentSummary(summary, newsArticleId);
                newSummary.PublishedDate = EnsureUtc(newSummary.PublishedDate);
                newSummary.SummaryDate = EnsureUtc(newSummary.SummaryDate);
                newSummary.CreatedAt = DateTime.UtcNow;
                newSummary.UpdatedAt = DateTime.UtcNow;

                context.AiSummaries.Add(newSummary);
                await context.SaveChangesAsync(cancellationToken);
                return newSummary;
            }
        }, nameof(AddOrUpdateAiSummaryAsync), cancellationToken);
    }

    public async Task<List<AiSummaryEntity>> AddOrUpdateAiSummariesAsync(List<AiContentSummary> summaries, CancellationToken cancellationToken = default)
    {
        if (!summaries.Any())
        {
            return new List<AiSummaryEntity>();
        }

        return await ExecuteWithContextAsync(async context =>
        {
            var results = new List<AiSummaryEntity>();

            // Get all URLs to check in one query
            var urls = summaries.Select(s => s.Url).ToList();
            
            // Get existing summaries
            var existingSummaries = await context.AiSummaries
                .Where(s => urls.Contains(s.Url))
                .ToListAsync(cancellationToken);

            // Get corresponding news articles to link them
            var newsArticles = await context.NewsArticles
                .Where(n => urls.Contains(n.Url))
                .ToListAsync(cancellationToken);

            var existingUrls = existingSummaries.ToDictionary(s => s.Url, s => s);
            var newsArticlesByUrl = newsArticles.ToDictionary(n => n.Url, n => n);

            foreach (var summary in summaries)
            {
                try
                {
                    // Get the corresponding news article ID
                    int? newsArticleId = newsArticlesByUrl.TryGetValue(summary.Url, out var newsArticle) 
                        ? newsArticle.Id 
                        : null;

                    if (existingUrls.TryGetValue(summary.Url, out var existing))
                    {
                        // Update existing
                        existing.Title = summary.Title;
                        existing.Source = summary.Source;
                        existing.PublishedDate = EnsureUtc(summary.PublishedDate);
                        existing.Summary = summary.Summary;
                        existing.AiGenerated = summary.AiGenerated;
                        existing.SummaryDate = EnsureUtc(summary.SummaryDate);
                        existing.UpdatedAt = DateTime.UtcNow;
                        existing.NewsArticleId = newsArticleId; // Link to news article
                        
                        results.Add(existing);
                    }
                    else
                    {
                        // Add new
                        var newSummary = AiSummaryEntity.FromAiContentSummary(summary, newsArticleId);
                        newSummary.PublishedDate = EnsureUtc(newSummary.PublishedDate);
                        newSummary.SummaryDate = EnsureUtc(newSummary.SummaryDate);
                        newSummary.CreatedAt = DateTime.UtcNow;
                        newSummary.UpdatedAt = DateTime.UtcNow;

                        context.AiSummaries.Add(newSummary);
                        results.Add(newSummary);
                    }

                    // Save entities if present
                    if (summary.Entities != null && summary.Entities.Any())
                    {
                        var article = newsArticle ?? await context.NewsArticles.FirstOrDefaultAsync(n => n.Url == summary.Url, cancellationToken);
                        if (article != null)
                        {
                            await AddEntitiesToArticleAsync(article.Id, summary.Entities, cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing AI summary for URL: {Url}", summary.Url);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            return results;
        }, nameof(AddOrUpdateAiSummariesAsync), cancellationToken);
    }

    public async Task<HashSet<string>> GetExistingAiSummaryUrlsAsync(IEnumerable<string> urls, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var uniqueUrls = urls.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().ToList();
            if (!uniqueUrls.Any()) return new HashSet<string>();

            var existingUrls = await context.AiSummaries
                .AsNoTracking()
                .Where(s => uniqueUrls.Contains(s.Url))
                .Select(s => s.Url)
                .ToListAsync(cancellationToken);

            return new HashSet<string>(existingUrls);
        }, nameof(GetExistingAiSummaryUrlsAsync), cancellationToken);
    }

    // Newsletter Subscribers
    public async Task<NewsletterSubscriber?> GetSubscriberByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NewsletterSubscribers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken),
            nameof(GetSubscriberByEmailAsync), cancellationToken);
    }

    public async Task<List<NewsletterSubscriber>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NewsletterSubscribers
                    .AsNoTracking()
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Email)
                    .ToListAsync(cancellationToken),
            nameof(GetActiveSubscribersAsync), cancellationToken);
    }

    public async Task<List<NewsletterSubscriber>> GetAllSubscribersAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NewsletterSubscribers
                    .AsNoTracking()
                    .OrderByDescending(s => s.SubscribedAt)
                    .ToListAsync(cancellationToken),
            nameof(GetAllSubscribersAsync), cancellationToken);
    }

    public async Task<NewsletterSubscriber> SubscribeAsync(string email, string source = "Website", bool isActive = true, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var existing = await context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            if (existing != null)
            {
                // Reactivate if unsubscribed AND we want to activate immediately
                // If isActive is false (pending confirmation), we should probably NOT reactivate immediately but generate a token?
                // But for simplicity, if they exist, we return them.
                // If they are inactive and we want to subscribe them (isActive=true), we reactivate.
                // If they are inactive and we pass isActive=false, we leave them inactive (waiting for confirmation).
                
                if (!existing.IsActive && isActive)
                {
                    existing.Resubscribe();
                    await context.SaveChangesAsync(cancellationToken);
                }
                return existing;
            }

            // Create new subscriber
            var subscriber = new NewsletterSubscriber
            {
                Email = email.ToLower(),
                Source = source,
                SubscribedAt = DateTime.UtcNow,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.NewsletterSubscribers.Add(subscriber);
            await context.SaveChangesAsync(cancellationToken);
            return subscriber;
        }, nameof(SubscribeAsync), cancellationToken);
    }

    public async Task<NewsletterSubscriber> UnsubscribeAsync(string email, string? reason = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var subscriber = await context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            if (subscriber == null)
            {
                throw new InvalidOperationException($"Subscriber with email {email} not found");
            }

            subscriber.Unsubscribe(reason);
            await context.SaveChangesAsync(cancellationToken);
            return subscriber;
        }, nameof(UnsubscribeAsync), cancellationToken);
    }

    public async Task<bool> IsSubscribedAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var subscriber = await context.NewsletterSubscribers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            return subscriber != null && subscriber.IsActive;
        }, nameof(IsSubscribedAsync), cancellationToken);
    }

    public async Task IncrementEmailSentAsync(string email, CancellationToken cancellationToken = default)
    {
        await ExecuteWithContextAsync(async context =>
        {
            var subscriber = await context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            if (subscriber != null && subscriber.IsActive)
            {
                subscriber.IncrementEmailSent();
                await context.SaveChangesAsync(cancellationToken);
            }

            return Task.CompletedTask;
        }, nameof(IncrementEmailSentAsync), cancellationToken);
    }

    public async Task<int> GetActiveSubscribersCountAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NewsletterSubscribers
                    .CountAsync(s => s.IsActive, cancellationToken),
            nameof(GetActiveSubscribersCountAsync), cancellationToken);
    }

    public async Task<string> GenerateUnsubscribeTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var subscriber = await context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            if (subscriber == null)
            {
                throw new InvalidOperationException($"Subscriber with email {email} not found");
            }

            // Check if there's already a valid token
            if (!string.IsNullOrEmpty(subscriber.UnsubscribeToken) && 
                subscriber.UnsubscribeTokenExpiresAt.HasValue &&
                subscriber.UnsubscribeTokenExpiresAt.Value > DateTime.UtcNow)
            {
                return subscriber.UnsubscribeToken;
            }

            subscriber.GenerateUnsubscribeToken();
            await context.SaveChangesAsync(cancellationToken);

            return subscriber.UnsubscribeToken ?? throw new InvalidOperationException("Failed to generate token");
        }, nameof(GenerateUnsubscribeTokenAsync), cancellationToken);
    }

    public async Task<string> GenerateConfirmationTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var subscriber = await context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            if (subscriber == null)
            {
                throw new InvalidOperationException($"Subscriber with email {email} not found");
            }

            // Check if there's already a valid token
            if (!string.IsNullOrEmpty(subscriber.ConfirmationToken) && 
                subscriber.ConfirmationTokenExpiresAt.HasValue &&
                subscriber.ConfirmationTokenExpiresAt.Value > DateTime.UtcNow)
            {
                return subscriber.ConfirmationToken;
            }

            subscriber.GenerateConfirmationToken();
            await context.SaveChangesAsync(cancellationToken);

            return subscriber.ConfirmationToken ?? throw new InvalidOperationException("Failed to generate token");
        }, nameof(GenerateConfirmationTokenAsync), cancellationToken);
    }

    public async Task<bool> HasValidUnsubscribeTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var subscriber = await context.NewsletterSubscribers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            if (subscriber == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(subscriber.UnsubscribeToken) &&
                   subscriber.UnsubscribeTokenExpiresAt.HasValue &&
                   subscriber.UnsubscribeTokenExpiresAt.Value > DateTime.UtcNow;
        }, nameof(HasValidUnsubscribeTokenAsync), cancellationToken);
    }

    public async Task<NewsletterSubscriber?> GetSubscriberByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NewsletterSubscribers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UnsubscribeToken == token.ToLower(), cancellationToken),
            nameof(GetSubscriberByTokenAsync), cancellationToken);
    }

    public async Task<NewsletterSubscriber?> GetSubscriberByConfirmationTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NewsletterSubscribers
                    .FirstOrDefaultAsync(s => s.ConfirmationToken == token.ToLower(), cancellationToken),
            nameof(GetSubscriberByConfirmationTokenAsync), cancellationToken);
    }

    public async Task<bool> UnsubscribeWithTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var normalizedToken = token.ToLower();
            var subscriber = await context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.UnsubscribeToken == normalizedToken, cancellationToken);

            if (subscriber == null)
            {
                _logger.LogWarning("No subscriber found with token: {Token}", normalizedToken.Substring(0, Math.Min(10, normalizedToken.Length)));
                return false;
            }

            if (!subscriber.IsUnsubscribeTokenValid(token))
            {
                _logger.LogWarning("Token invalid or expired for subscriber: {Email}", subscriber.Email);
                return false;
            }

            subscriber.Unsubscribe("Unsubscribed via email link");
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }, nameof(UnsubscribeWithTokenAsync), cancellationToken);
    }

    public async Task<bool> ConfirmSubscriptionAsync(string token, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var normalizedToken = token.ToLower();
            var subscriber = await context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.ConfirmationToken == normalizedToken, cancellationToken);

            if (subscriber == null)
            {
                _logger.LogWarning("No subscriber found with confirmation token: {Token}", normalizedToken.Substring(0, Math.Min(10, normalizedToken.Length)));
                return false;
            }

            if (!subscriber.IsConfirmationTokenValid(token))
            {
                _logger.LogWarning("Confirmation token invalid or expired for subscriber: {Email}", subscriber.Email);
                return false;
            }

            subscriber.ConfirmSubscription();
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }, nameof(ConfirmSubscriptionAsync), cancellationToken);
    }

    // Daily Newsletters
    public async Task<DailyNewsletter?> GetNewsletterByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.DailyNewsletters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(n => n.NewsletterDate == date, cancellationToken),
            nameof(GetNewsletterByDateAsync), cancellationToken);
    }

    public async Task<DailyNewsletter?> GetTodayNewsletterAsync(CancellationToken cancellationToken = default)
    {
        var today = DailyNewsletter.GetNewsletterDateFromUtc(DateTime.UtcNow);
        return await GetNewsletterByDateAsync(today, cancellationToken);
    }

    public async Task<List<DailyNewsletter>> GetRecentNewslettersAsync(int count = 30, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.DailyNewsletters
                    .AsNoTracking()
                    .OrderByDescending(n => n.NewsletterDate)
                    .Take(count)
                    .ToListAsync(cancellationToken),
            nameof(GetRecentNewslettersAsync), cancellationToken);
    }

    public async Task<DailyNewsletter> CreateOrUpdateNewsletterAsync(DailyNewsletter newsletter, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var existing = await context.DailyNewsletters
                .FirstOrDefaultAsync(n => n.NewsletterDate == newsletter.NewsletterDate, cancellationToken);

            if (existing != null)
            {
                // Update existing newsletter (if not sent yet)
                if (!existing.IsSent)
                {
                    existing.Subject = newsletter.Subject;
                    existing.HtmlContent = newsletter.HtmlContent;
                    existing.TextContent = newsletter.TextContent;
                    existing.SummaryCount = newsletter.SummaryCount;
                    existing.UpdatedAt = DateTime.UtcNow;

                    await context.SaveChangesAsync(cancellationToken);
                    return existing;
                }
                else
                {
                    throw new InvalidOperationException($"Newsletter for {newsletter.NewsletterDate} has already been sent and cannot be updated");
                }
            }

            // Create new newsletter
            context.DailyNewsletters.Add(newsletter);
            await context.SaveChangesAsync(cancellationToken);
            return newsletter;
        }, nameof(CreateOrUpdateNewsletterAsync), cancellationToken);
    }

    public async Task MarkNewsletterAsSentAsync(DateOnly date, int recipientCount, CancellationToken cancellationToken = default)
    {
        await ExecuteWithContextAsync(async context =>
        {
            var newsletter = await context.DailyNewsletters
                .FirstOrDefaultAsync(n => n.NewsletterDate == date, cancellationToken);

            if (newsletter != null)
            {
                newsletter.MarkAsSent(recipientCount);
                await context.SaveChangesAsync(cancellationToken);
            }

            return Task.CompletedTask;
        }, nameof(MarkNewsletterAsSentAsync), cancellationToken);
    }

    public async Task<bool> HasNewsletterForDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.DailyNewsletters
                    .AnyAsync(n => n.NewsletterDate == date, cancellationToken),
            nameof(HasNewsletterForDateAsync), cancellationToken);
    }

    public async Task<DominantTheme?> GetDominantThemeByDateAsync(DateOnly generationDate, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.DominantThemes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.GenerationDate == generationDate, cancellationToken),
            nameof(GetDominantThemeByDateAsync), cancellationToken);
    }

    public async Task<DominantTheme> AddOrUpdateDominantThemeAsync(DateOnly generationDate, string theme, string? rationale, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
        {
            var existing = await context.DominantThemes
                .FirstOrDefaultAsync(t => t.GenerationDate == generationDate, cancellationToken);

            if (existing != null)
            {
                existing.Theme = theme;
                existing.Rationale = rationale;
                existing.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync(cancellationToken);
                return existing;
            }

            var entity = DominantTheme.Create(generationDate, theme, rationale);
            context.DominantThemes.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            return entity;
        }, nameof(AddOrUpdateDominantThemeAsync), cancellationToken);
    }

    // Named Entities (NER)
    public async Task<List<NamedEntity>> GetAllNamedEntitiesAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NamedEntities
                    .AsNoTracking()
                    .OrderBy(e => e.Name)
                    .ToListAsync(cancellationToken),
            nameof(GetAllNamedEntitiesAsync), cancellationToken);
    }

    public async Task<List<NamedEntity>> GetEntitiesWithArticlesAsync(int articleCount = 100, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NamedEntities
                    .Include(e => e.Articles.OrderByDescending(a => a.PublishedDate).Take(articleCount))
                    .Where(e => e.Articles.Any())
                    .ToListAsync(cancellationToken),
            nameof(GetEntitiesWithArticlesAsync), cancellationToken);
    }

    public async Task AddEntitiesToArticleAsync(int articleId, List<string> entityNames, CancellationToken cancellationToken = default)
    {
        if (entityNames == null || !entityNames.Any()) return;

        await ExecuteWithContextAsync(async context =>
        {
            var article = await context.NewsArticles
                .Include(a => a.Entities)
                .FirstOrDefaultAsync(a => a.Id == articleId, cancellationToken);

            if (article == null) return Task.CompletedTask;

            foreach (var name in entityNames)
            {
                var normalizedName = name.Trim();
                if (string.IsNullOrEmpty(normalizedName)) continue;

                var entity = await context.NamedEntities
                    .FirstOrDefaultAsync(e => e.Name == normalizedName, cancellationToken);

                if (entity == null)
                {
                    entity = new NamedEntity { Name = normalizedName };
                    context.NamedEntities.Add(entity);
                }

                if (!article.Entities.Any(e => e.Name == normalizedName))
                {
                    article.Entities.Add(entity);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            return Task.CompletedTask;
        }, nameof(AddEntitiesToArticleAsync), cancellationToken);
    }

    public async Task<int> GetNamedEntityCountAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithContextAsync(async context =>
                await context.NamedEntities.CountAsync(cancellationToken),
            nameof(GetNamedEntityCountAsync), cancellationToken);
    }
}
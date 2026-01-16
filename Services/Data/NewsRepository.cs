using Microsoft.EntityFrameworkCore;
using VeilleNet.Data;
using VeilleNet.Models;
using VeilleNet.Models.Entities;

namespace VeilleNet.Services.Data;

public class NewsRepository : INewsRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NewsRepository> _logger;

    public NewsRepository(ApplicationDbContext context, ILogger<NewsRepository> logger)
    {
        _context = context;
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
        return await ExecuteWithRetryAsync(async () =>
            await _context.NewsArticles
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Url == url, cancellationToken),
            nameof(GetNewsArticleByUrlAsync));
    }

    public async Task<List<NewsArticle>> GetRecentNewsArticlesAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.NewsArticles
                .AsNoTracking()
                .OrderByDescending(n => n.PublishedDate)
                .Take(count)
                .ToListAsync(cancellationToken),
            nameof(GetRecentNewsArticlesAsync));
    }

    public async Task<NewsArticle> AddNewsArticleAsync(NewsArticle article, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            // Ensure all DateTime values are UTC
            article.PublishedDate = EnsureUtc(article.PublishedDate);
            article.CreatedAt = EnsureUtc(article.CreatedAt);
            article.UpdatedAt = EnsureUtc(article.UpdatedAt);

            _context.NewsArticles.Add(article);
            await _context.SaveChangesAsync(cancellationToken);
            return article;
        }, nameof(AddNewsArticleAsync));
    }

    public async Task<NewsArticle> UpdateNewsArticleAsync(NewsArticle article, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            // Ensure all DateTime values are UTC
            article.PublishedDate = EnsureUtc(article.PublishedDate);
            article.UpdatedAt = EnsureUtc(DateTime.UtcNow);

            _context.Entry(article).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
            return article;
        }, nameof(UpdateNewsArticleAsync));
    }

    public async Task<List<NewsArticle>> AddOrUpdateNewsArticlesAsync(List<BaseNews> news, CancellationToken cancellationToken = default)
    {
        if (!news.Any())
        {
            return new List<NewsArticle>();
        }

        return await ExecuteWithRetryAsync(async () =>
        {
            var results = new List<NewsArticle>();
            
            // Get all URLs to check in one query
            var urls = news.Select(n => n.Url).ToList();
            var existingArticles = await _context.NewsArticles
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
                    
                    _context.NewsArticles.Add(newArticle);
                    results.Add(newArticle);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return results;
        }, nameof(AddOrUpdateNewsArticlesAsync));
    }

    public async Task<(List<NewsArticle> Items, int TotalCount)> SearchNewsArticlesAsync(string? keyword, DateTime? startDate, DateTime? endDate, string? source, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var query = _context.NewsArticles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var pattern = $"%{keyword}%";
                query = query.Where(n => EF.Functions.ILike(n.Title, pattern) || EF.Functions.ILike(n.Summary, pattern));
            }

            if (startDate.HasValue)
            {
                var startUtc = EnsureUtc(startDate.Value);
                query = query.Where(n => n.PublishedDate >= startUtc);
            }

            if (endDate.HasValue)
            {
                var endUtc = EnsureUtc(endDate.Value);
                query = query.Where(n => n.PublishedDate <= endUtc);
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
        }, nameof(SearchNewsArticlesAsync));
    }

    public async Task<List<string>> GetAllNewsSourcesAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.NewsArticles
                .AsNoTracking()
                .Select(n => n.Source)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync(cancellationToken),
            nameof(GetAllNewsSourcesAsync));
    }

    // AI Summaries
    public async Task<AiSummaryEntity?> GetAiSummaryByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.AiSummaries
                .FirstOrDefaultAsync(s => s.Url == url, cancellationToken),
            nameof(GetAiSummaryByUrlAsync));
    }

    public async Task<List<AiSummaryEntity>> GetRecentAiSummariesAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.AiSummaries
                .AsNoTracking()
                .OrderByDescending(s => s.SummaryDate)
                .Take(count)
                .ToListAsync(cancellationToken),
            nameof(GetRecentAiSummariesAsync));
    }

    public async Task<List<AiSummaryEntity>> GetAiSummariesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.AiSummaries
                .AsNoTracking()
                .Where(s => s.SummaryDate >= startDate && s.SummaryDate <= endDate)
                .OrderByDescending(s => s.SummaryDate)
                .ToListAsync(cancellationToken),
            nameof(GetAiSummariesByDateRangeAsync));
    }

    public async Task<AiSummaryEntity> AddAiSummaryAsync(AiSummaryEntity summary, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            // Ensure all DateTime values are UTC
            summary.PublishedDate = EnsureUtc(summary.PublishedDate);
            summary.SummaryDate = EnsureUtc(summary.SummaryDate);
            summary.CreatedAt = DateTime.UtcNow;
            summary.UpdatedAt = DateTime.UtcNow;

            _context.AiSummaries.Add(summary);
            await _context.SaveChangesAsync(cancellationToken);
            return summary;
        }, nameof(AddAiSummaryAsync));
    }

    public async Task<AiSummaryEntity> UpdateAiSummaryAsync(AiSummaryEntity summary, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            // Ensure all DateTime values are UTC
            summary.PublishedDate = EnsureUtc(summary.PublishedDate);
            summary.SummaryDate = EnsureUtc(summary.SummaryDate);
            summary.UpdatedAt = DateTime.UtcNow;

            _context.Entry(summary).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
            return summary;
        }, nameof(UpdateAiSummaryAsync));
    }

    public async Task<AiSummaryEntity> AddOrUpdateAiSummaryAsync(AiContentSummary summary, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var existing = await _context.AiSummaries
                .FirstOrDefaultAsync(s => s.Url == summary.Url, cancellationToken);

            // Get the corresponding news article
            var newsArticle = await _context.NewsArticles
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

                await _context.SaveChangesAsync(cancellationToken);
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

                _context.AiSummaries.Add(newSummary);
                await _context.SaveChangesAsync(cancellationToken);
                return newSummary;
            }
        }, nameof(AddOrUpdateAiSummaryAsync));
    }

    public async Task<List<AiSummaryEntity>> AddOrUpdateAiSummariesAsync(List<AiContentSummary> summaries, CancellationToken cancellationToken = default)
    {
        if (!summaries.Any())
        {
            return new List<AiSummaryEntity>();
        }

        return await ExecuteWithRetryAsync(async () =>
        {
            var results = new List<AiSummaryEntity>();

            // Get all URLs to check in one query
            var urls = summaries.Select(s => s.Url).ToList();
            
            // Get existing summaries
            var existingSummaries = await _context.AiSummaries
                .Where(s => urls.Contains(s.Url))
                .ToListAsync(cancellationToken);

            // Get corresponding news articles to link them
            var newsArticles = await _context.NewsArticles
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

                        _context.AiSummaries.Add(newSummary);
                        results.Add(newSummary);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing AI summary for URL: {Url}", summary.Url);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return results;
        }, nameof(AddOrUpdateAiSummariesAsync));
    }

    // Newsletter Subscribers
    public async Task<NewsletterSubscriber?> GetSubscriberByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.NewsletterSubscribers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken),
            nameof(GetSubscriberByEmailAsync));
    }

    public async Task<List<NewsletterSubscriber>> GetActiveSubscribersAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.NewsletterSubscribers
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Email)
                .ToListAsync(cancellationToken),
            nameof(GetActiveSubscribersAsync));
    }

    public async Task<List<NewsletterSubscriber>> GetAllSubscribersAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.NewsletterSubscribers
                .AsNoTracking()
                .OrderByDescending(s => s.SubscribedAt)
                .ToListAsync(cancellationToken),
            nameof(GetAllSubscribersAsync));
    }

    public async Task<NewsletterSubscriber> SubscribeAsync(string email, string source = "Website", CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var existing = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            if (existing != null)
            {
                // Reactivate if unsubscribed
                if (!existing.IsActive)
                {
                    existing.Resubscribe();
                    await _context.SaveChangesAsync(cancellationToken);
                }
                return existing;
            }

            // Create new subscriber
            var subscriber = new NewsletterSubscriber
            {
                Email = email.ToLower(),
                Source = source,
                SubscribedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.NewsletterSubscribers.Add(subscriber);
            await _context.SaveChangesAsync(cancellationToken);
            return subscriber;
        }, nameof(SubscribeAsync));
    }

    public async Task<NewsletterSubscriber> UnsubscribeAsync(string email, string? reason = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            if (subscriber == null)
            {
                throw new InvalidOperationException($"Subscriber with email {email} not found");
            }

            subscriber.Unsubscribe(reason);
            await _context.SaveChangesAsync(cancellationToken);
            return subscriber;
        }, nameof(UnsubscribeAsync));
    }

    public async Task<bool> IsSubscribedAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var subscriber = await _context.NewsletterSubscribers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            return subscriber != null && subscriber.IsActive;
        }, nameof(IsSubscribedAsync));
    }

    public async Task IncrementEmailSentAsync(string email, CancellationToken cancellationToken = default)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            if (subscriber != null && subscriber.IsActive)
            {
                subscriber.IncrementEmailSent();
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Task.CompletedTask;
        }, nameof(IncrementEmailSentAsync));
    }

    public async Task<int> GetActiveSubscribersCountAsync(CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.NewsletterSubscribers
                .CountAsync(s => s.IsActive, cancellationToken),
            nameof(GetActiveSubscribersCountAsync));
    }

    public async Task<string> GenerateUnsubscribeTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var subscriber = await _context.NewsletterSubscribers
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
                // Return existing valid token instead of generating a new one
                return subscriber.UnsubscribeToken;
            }

            // Generate new token only if no valid token exists
            subscriber.GenerateUnsubscribeToken();
            await _context.SaveChangesAsync(cancellationToken);

            return subscriber.UnsubscribeToken ?? throw new InvalidOperationException("Failed to generate token");
        }, nameof(GenerateUnsubscribeTokenAsync));
    }

    public async Task<bool> HasValidUnsubscribeTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var subscriber = await _context.NewsletterSubscribers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);

            if (subscriber == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(subscriber.UnsubscribeToken) &&
                   subscriber.UnsubscribeTokenExpiresAt.HasValue &&
                   subscriber.UnsubscribeTokenExpiresAt.Value > DateTime.UtcNow;
        }, nameof(HasValidUnsubscribeTokenAsync));
    }

    public async Task<NewsletterSubscriber?> GetSubscriberByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.NewsletterSubscribers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UnsubscribeToken == token.ToLower(), cancellationToken),
            nameof(GetSubscriberByTokenAsync));
    }

    public async Task<bool> UnsubscribeWithTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.UnsubscribeToken == token.ToLower(), cancellationToken);

            if (subscriber == null)
            {
                return false;
            }

            if (!subscriber.IsUnsubscribeTokenValid(token))
            {
                return false;
            }

            subscriber.Unsubscribe("Unsubscribed via email link");
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }, nameof(UnsubscribeWithTokenAsync));
    }

    // Daily Newsletters
    public async Task<DailyNewsletter?> GetNewsletterByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.DailyNewsletters
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NewsletterDate == date, cancellationToken),
            nameof(GetNewsletterByDateAsync));
    }

    public async Task<DailyNewsletter?> GetTodayNewsletterAsync(CancellationToken cancellationToken = default)
    {
        var today = DailyNewsletter.GetNewsletterDateFromUtc(DateTime.UtcNow);
        return await GetNewsletterByDateAsync(today, cancellationToken);
    }

    public async Task<List<DailyNewsletter>> GetRecentNewslettersAsync(int count = 30, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.DailyNewsletters
                .AsNoTracking()
                .OrderByDescending(n => n.NewsletterDate)
                .Take(count)
                .ToListAsync(cancellationToken),
            nameof(GetRecentNewslettersAsync));
    }

    public async Task<DailyNewsletter> CreateOrUpdateNewsletterAsync(DailyNewsletter newsletter, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var existing = await _context.DailyNewsletters
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
                    
                    await _context.SaveChangesAsync(cancellationToken);
                    return existing;
                }
                else
                {
                    throw new InvalidOperationException($"Newsletter for {newsletter.NewsletterDate} has already been sent and cannot be updated");
                }
            }

            // Create new newsletter
            _context.DailyNewsletters.Add(newsletter);
            await _context.SaveChangesAsync(cancellationToken);
            return newsletter;
        }, nameof(CreateOrUpdateNewsletterAsync));
    }

    public async Task MarkNewsletterAsSentAsync(DateOnly date, int recipientCount, CancellationToken cancellationToken = default)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            var newsletter = await _context.DailyNewsletters
                .FirstOrDefaultAsync(n => n.NewsletterDate == date, cancellationToken);

            if (newsletter != null)
            {
                newsletter.MarkAsSent(recipientCount);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Task.CompletedTask;
        }, nameof(MarkNewsletterAsSentAsync));
    }

    public async Task<bool> HasNewsletterForDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.DailyNewsletters
                .AnyAsync(n => n.NewsletterDate == date, cancellationToken),
            nameof(HasNewsletterForDateAsync));
    }

    public async Task<DominantTheme?> GetDominantThemeByDateAsync(DateOnly generationDate, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
            await _context.DominantThemes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.GenerationDate == generationDate, cancellationToken),
            nameof(GetDominantThemeByDateAsync));
    }

    public async Task<DominantTheme> AddOrUpdateDominantThemeAsync(DateOnly generationDate, string theme, string? rationale, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var existing = await _context.DominantThemes
                .FirstOrDefaultAsync(t => t.GenerationDate == generationDate, cancellationToken);

            if (existing != null)
            {
                existing.Theme = theme;
                existing.Rationale = rationale;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
                return existing;
            }

            var entity = DominantTheme.Create(generationDate, theme, rationale);
            _context.DominantThemes.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }, nameof(AddOrUpdateDominantThemeAsync));
    }
}
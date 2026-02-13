using VeilleNet.Models;
using VeilleNet.Models.Entities;

namespace VeilleNet.Services.Data;

public interface INewsRepository
{
    // News Articles
    Task<NewsArticle?> GetNewsArticleByUrlAsync(string url, CancellationToken cancellationToken = default);
    Task<List<NewsArticle>> GetRecentNewsArticlesAsync(int count = 50, CancellationToken cancellationToken = default);
    Task<List<NewsArticle>> GetRecentAiSummarizedNewsArticlesAsync(int count = 100, CancellationToken cancellationToken = default);
    Task<NewsArticle> AddNewsArticleAsync(NewsArticle article, CancellationToken cancellationToken = default);
    Task<NewsArticle> UpdateNewsArticleAsync(NewsArticle article, CancellationToken cancellationToken = default);
    Task<List<NewsArticle>> AddOrUpdateNewsArticlesAsync(List<BaseNews> news, CancellationToken cancellationToken = default);
    Task<(List<NewsArticle> Items, int TotalCount)> SearchNewsArticlesAsync(string? keyword, DateTime? startDate, DateTime? endDate, string? source, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<List<string>> GetAllNewsSourcesAsync(CancellationToken cancellationToken = default);

    // Dominant Themes
    Task<DominantTheme?> GetDominantThemeByDateAsync(DateOnly generationDate, CancellationToken cancellationToken = default);
    Task<DominantTheme> AddOrUpdateDominantThemeAsync(DateOnly generationDate, string theme, string? rationale, CancellationToken cancellationToken = default);

    // AI Summaries
    Task<AiSummaryEntity?> GetAiSummaryByUrlAsync(string url, CancellationToken cancellationToken = default);
    Task<List<AiSummaryEntity>> GetRecentAiSummariesAsync(int count = 50, CancellationToken cancellationToken = default);
    Task<List<AiSummaryEntity>> GetAiSummariesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<AiSummaryEntity> AddAiSummaryAsync(AiSummaryEntity summary, CancellationToken cancellationToken = default);
    Task<AiSummaryEntity> UpdateAiSummaryAsync(AiSummaryEntity summary, CancellationToken cancellationToken = default);
    Task<AiSummaryEntity> AddOrUpdateAiSummaryAsync(AiContentSummary summary, CancellationToken cancellationToken = default);
    Task<List<AiSummaryEntity>> AddOrUpdateAiSummariesAsync(List<AiContentSummary> summaries, CancellationToken cancellationToken = default);
    Task<HashSet<string>> GetExistingAiSummaryUrlsAsync(IEnumerable<string> urls, CancellationToken cancellationToken = default);

    // Newsletter Subscribers
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

    // Daily Newsletters
    Task<DailyNewsletter?> GetNewsletterByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<DailyNewsletter?> GetTodayNewsletterAsync(CancellationToken cancellationToken = default);
    Task<List<DailyNewsletter>> GetRecentNewslettersAsync(int count = 30, CancellationToken cancellationToken = default);
    Task<DailyNewsletter> CreateOrUpdateNewsletterAsync(DailyNewsletter newsletter, CancellationToken cancellationToken = default);
    Task MarkNewsletterAsSentAsync(DateOnly date, int recipientCount, CancellationToken cancellationToken = default);
    Task<bool> HasNewsletterForDateAsync(DateOnly date, CancellationToken cancellationToken = default);

    // Named Entities (NER)
    Task<List<NamedEntity>> GetAllNamedEntitiesAsync(CancellationToken cancellationToken = default);
    Task<List<NamedEntity>> GetEntitiesWithArticlesAsync(int articleCount = 100, CancellationToken cancellationToken = default);
    Task AddEntitiesToArticleAsync(int articleId, List<string> entityNames, CancellationToken cancellationToken = default);
    Task<int> GetNamedEntityCountAsync(CancellationToken cancellationToken = default);
}

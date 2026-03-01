using VeilleNet.Models;
using VeilleNet.Services.Data;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.News;

/// <summary>
/// Feed definition for a news source.
/// </summary>
public record FeedDefinition(
    string Name,
    string Url,
    string DefaultImage,
    string Category = "",
    int MaxItems = 10,
    Func<BaseNews, bool>? Filter = null);

/// <summary>
/// Base class that captures the common cache → fetch → enrich → store pattern
/// shared by AINewsService, BlogAggregationService, and WinFormNewsService.
/// </summary>
public abstract class BaseNewsAggregationService
{
    private readonly ICacheService _cacheService;
    private readonly IFeedService _feedService;
    private readonly IAiSummaryRepository _aiSummaryRepository;
    private readonly ILogger _logger;

    protected abstract string CacheKey { get; }
    protected virtual TimeSpan CacheExpiration => TimeSpan.FromHours(1);
    protected virtual int TakeCount => 20;

    /// <summary>
    /// Defines the feeds this service aggregates.
    /// </summary>
    protected abstract IReadOnlyList<FeedDefinition> Feeds { get; }

    protected BaseNewsAggregationService(
        ICacheService cacheService,
        IFeedService feedService,
        IAiSummaryRepository aiSummaryRepository,
        ILogger logger)
    {
        _cacheService = cacheService;
        _feedService = feedService;
        _aiSummaryRepository = aiSummaryRepository;
        _logger = logger;
    }

    protected async Task<List<BaseNews>> FetchAndCacheAsync()
    {
        var cached = _cacheService.Get<List<BaseNews>>(CacheKey);
        if (cached != null)
            return cached;

        var allNews = new List<BaseNews>();

        foreach (var feed in Feeds)
        {
            try
            {
                var items = await _feedService.FetchNewsFeedAsync(
                    feed.Name, feed.Url, feed.DefaultImage, feed.MaxItems, feed.Category, feed.Filter);
                allNews.AddRange(items);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch feed {FeedName} ({FeedUrl})", feed.Name, feed.Url);
            }
        }

        allNews = allNews.OrderByDescending(n => n.PublishedDate).Take(TakeCount).ToList();

        // Enrich with HasAiSummary (batch check)
        var urls = allNews.Select(n => n.Url).ToList();
        var existingSummaries = await _aiSummaryRepository.GetExistingAiSummaryUrlsAsync(urls);
        foreach (var news in allNews)
        {
            news.HasAiSummary = existingSummaries.Contains(news.Url);
        }

        _cacheService.Set(CacheKey, allNews, CacheExpiration);
        return allNews;
    }
}

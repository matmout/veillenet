using VeilleNet.Models;
using VeilleNet.Services.Data;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.News;

public interface IWinFormNewsService
{
    Task<List<BaseNews>> GetLatestWinFormNewsAsync();
}

public class WinFormNewsService : BaseNewsAggregationService, IWinFormNewsService
{
    protected override string CacheKey => "WinFormNews";

    protected override IReadOnlyList<FeedDefinition> Feeds { get; } = new List<FeedDefinition>
    {
        new("DevExpress", "https://community.devexpress.com/Blogs/MainFeed", "https://www.devexpress.com/favicon.ico", "DevExpress", MaxItems: 20),
    };

    public WinFormNewsService(
        ICacheService cacheService,
        IFeedService feedService,
        IAiSummaryRepository aiSummaryRepository,
        ILogger<WinFormNewsService> logger)
        : base(cacheService, feedService, aiSummaryRepository, logger) { }

    public Task<List<BaseNews>> GetLatestWinFormNewsAsync() => FetchAndCacheAsync();
}

using VeilleNet.Models;
using VeilleNet.Services.Data;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.News;

public interface IBlogAggregationService
{
    Task<List<BaseNews>> GetLatestPostsAsync();
}

public class BlogAggregationService : BaseNewsAggregationService, IBlogAggregationService
{
    private const string DefaultMicrosoftImage = "/images/mcp/microsoft.png";

    protected override string CacheKey => "BlogPosts";

    protected override IReadOnlyList<FeedDefinition> Feeds { get; } = new List<FeedDefinition>
    {
        new(".NET Blog", "https://devblogs.microsoft.com/dotnet/feed/", DefaultMicrosoftImage, "Microsoft"),
        new("ASP.NET Blog", "https://devblogs.microsoft.com/dotnet/category/aspnet/feed/", DefaultMicrosoftImage, "Microsoft"),
        new("Visual Studio Blog", "https://devblogs.microsoft.com/visualstudio/feed/", DefaultMicrosoftImage, "Microsoft"),
        new("C# Blog", "https://devblogs.microsoft.com/dotnet/category/csharp/feed/", DefaultMicrosoftImage, "Microsoft"),
    };

    public BlogAggregationService(
        ICacheService cacheService,
        IFeedService feedService,
        IAiSummaryRepository aiSummaryRepository,
        ILogger<BlogAggregationService> logger)
        : base(cacheService, feedService, aiSummaryRepository, logger) { }

    public Task<List<BaseNews>> GetLatestPostsAsync() => FetchAndCacheAsync();
}

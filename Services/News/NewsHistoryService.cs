using VeilleNet.Models;
using VeilleNet.Services.Data;

namespace VeilleNet.Services.News;

public record NewsSearchResult(List<BaseNews> Items, int TotalCount);

public interface INewsHistoryService
{
    Task<NewsSearchResult> SearchAsync(string? keyword, DateTime? startDate, DateTime? endDate, string? source, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<List<BaseNews>> GetRecentAsync(int count = 20, CancellationToken cancellationToken = default);
    Task<List<string>> GetSourcesAsync(CancellationToken cancellationToken = default);
}

public class NewsHistoryService : INewsHistoryService
{
    private readonly INewsRepository _newsRepository;

    public NewsHistoryService(INewsRepository newsRepository)
    {
        _newsRepository = newsRepository;
    }

    public async Task<NewsSearchResult> SearchAsync(string? keyword, DateTime? startDate, DateTime? endDate, string? source, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(pageSize, 1, 200);
        var skip = Math.Max(0, (page - 1) * take);

        var (items, total) = await _newsRepository.SearchNewsArticlesAsync(keyword, startDate, endDate, source, skip, take, cancellationToken);
        var mapped = items.Select(MapToBaseNews).ToList();
        return new NewsSearchResult(mapped, total);
    }

    public async Task<List<BaseNews>> GetRecentAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        var (items, _) = await _newsRepository.SearchNewsArticlesAsync(null, null, null, null, 0, Math.Clamp(count, 1, 200), cancellationToken);
        return items.Select(MapToBaseNews).ToList();
    }

    public Task<List<string>> GetSourcesAsync(CancellationToken cancellationToken = default)
        => _newsRepository.GetAllNewsSourcesAsync(cancellationToken);

    private static BaseNews MapToBaseNews(Models.Entities.NewsArticle article)
    {
        return new BaseNews
        {
            Title = article.Title,
            Url = article.Url,
            Summary = article.Summary,
            PublishedDate = article.PublishedDate,
            Author = article.Author,
            Source = article.Source,
            Category = article.Category,
            Image = article.Image
        };
    }
}

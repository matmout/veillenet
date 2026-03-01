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
    private readonly IArticleRepository _articleRepository;
    private readonly IAiSummaryRepository _aiSummaryRepository;

    public NewsHistoryService(IArticleRepository articleRepository, IAiSummaryRepository aiSummaryRepository)
    {
        _articleRepository = articleRepository;
        _aiSummaryRepository = aiSummaryRepository;
    }

    public async Task<NewsSearchResult> SearchAsync(string? keyword, DateTime? startDate, DateTime? endDate, string? source, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(pageSize, 1, 200);
        var skip = Math.Max(0, (page - 1) * take);

        var (items, total) = await _articleRepository.SearchNewsArticlesAsync(keyword, startDate, endDate, source, skip, take, cancellationToken);
        
        var mapped = items.Select(MapToBaseNews).ToList();
        
         // Enrich with HasAiSummary
        var urls = mapped.Select(n => n.Url).ToList();
        var existingSummaries = await _aiSummaryRepository.GetExistingAiSummaryUrlsAsync(urls, cancellationToken);
        
        foreach (var news in mapped)
        {
            news.HasAiSummary = existingSummaries.Contains(news.Url);
        }
        
        return new NewsSearchResult(mapped, total);
    }

    public async Task<List<BaseNews>> GetRecentAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        var (items, _) = await _articleRepository.SearchNewsArticlesAsync(null, null, null, null, 0, Math.Clamp(count, 1, 200), cancellationToken);
        var mapped = items.Select(MapToBaseNews).ToList();

        // Enrich with HasAiSummary
        var urls = mapped.Select(n => n.Url).ToList();
        var existingSummaries = await _aiSummaryRepository.GetExistingAiSummaryUrlsAsync(urls, cancellationToken);
        
        foreach (var news in mapped)
        {
            news.HasAiSummary = existingSummaries.Contains(news.Url);
        }

        return mapped;
    }

    public Task<List<string>> GetSourcesAsync(CancellationToken cancellationToken = default)
        => _articleRepository.GetAllNewsSourcesAsync(cancellationToken);

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

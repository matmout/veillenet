using VeilleNet.Models;
using VeilleNet.Models.Entities;

namespace VeilleNet.Services.Data;

/// <summary>
/// Repository for news articles, search, and dominant themes.
/// </summary>
public interface IArticleRepository
{
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

    // Named Entities (NER)
    Task<List<NamedEntity>> GetAllNamedEntitiesAsync(CancellationToken cancellationToken = default);
    Task<List<NamedEntity>> GetEntitiesWithArticlesAsync(int articleCount = 100, CancellationToken cancellationToken = default);
    Task AddEntitiesToArticleAsync(int articleId, List<string> entityNames, CancellationToken cancellationToken = default);
    Task<int> GetNamedEntityCountAsync(CancellationToken cancellationToken = default);
}

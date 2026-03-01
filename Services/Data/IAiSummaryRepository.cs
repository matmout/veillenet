using VeilleNet.Models;
using VeilleNet.Models.Entities;

namespace VeilleNet.Services.Data;

/// <summary>
/// Repository for AI-generated content summaries.
/// </summary>
public interface IAiSummaryRepository
{
    Task<AiSummaryEntity?> GetAiSummaryByUrlAsync(string url, CancellationToken cancellationToken = default);
    Task<List<AiSummaryEntity>> GetRecentAiSummariesAsync(int count = 50, CancellationToken cancellationToken = default);
    Task<List<AiSummaryEntity>> GetAiSummariesByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<AiSummaryEntity> AddAiSummaryAsync(AiSummaryEntity summary, CancellationToken cancellationToken = default);
    Task<AiSummaryEntity> UpdateAiSummaryAsync(AiSummaryEntity summary, CancellationToken cancellationToken = default);
    Task<AiSummaryEntity> AddOrUpdateAiSummaryAsync(AiContentSummary summary, CancellationToken cancellationToken = default);
    Task<List<AiSummaryEntity>> AddOrUpdateAiSummariesAsync(List<AiContentSummary> summaries, CancellationToken cancellationToken = default);
    Task<HashSet<string>> GetExistingAiSummaryUrlsAsync(IEnumerable<string> urls, CancellationToken cancellationToken = default);
}

using VeilleNet.Models;

namespace VeilleNet.Services.Tools;

public static class SummaryFilter
{
    /// <summary>
    /// Select up to <paramref name="maxCount"/> summaries prioritizing distinct sources.
    /// Strategy:
    /// 1. Pick one summary per source (oldest or newest? we'll pick newest) until we reach maxCount.
    /// 2. If we still need more, fill with remaining summaries ordered by PublishedDate desc.
    /// This maximizes source diversity while returning the most recent items.
    /// </summary>
    public static List<AiContentSummary> SelectMaxDistinctSources(IEnumerable<AiContentSummary> summaries, int maxCount)
    {
        if (summaries == null) return new List<AiContentSummary>();

        var list = summaries.OrderByDescending(s => s.PublishedDate).ToList();
        var result = new List<AiContentSummary>(maxCount);
        var seenSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // First pass: pick newest per source
        foreach (var item in list)
        {
            if (result.Count >= maxCount) break;

            if (string.IsNullOrWhiteSpace(item.Source))
            {
                // Defer items without source to second pass
                continue;
            }

            if (!seenSources.Contains(item.Source))
            {
                result.Add(item);
                seenSources.Add(item.Source);
            }
        }

        if (result.Count < maxCount)
        {
            // Second pass: fill with remaining items (including those without source)
            foreach (var item in list)
            {
                if (result.Count >= maxCount) break;
                if (result.Contains(item)) continue;
                result.Add(item);
            }
        }

        return result;
    }
}

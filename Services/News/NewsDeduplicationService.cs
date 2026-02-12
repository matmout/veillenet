using VeilleNet.Models;
using VeilleNet.Models.Entities;

namespace VeilleNet.Services.News;

public interface INewsDeduplicationService
{
    /// <summary>
    /// Checks if a news item is a duplicate of any existing article.
    /// </summary>
    /// <param name="candidate">The news item to check.</param>
    /// <param name="existingArticles">The list of recently existing articles to check against.</param>
    /// <returns>True if a duplicate is found, otherwise false.</returns>
    bool IsDuplicate(BaseNews candidate, List<NewsArticle> existingArticles);
}

public class NewsDeduplicationService : INewsDeduplicationService
{
    // Threshold for Jaccard Similarity. 
    // 1.0 means exact match (after normalization).
    // 0.8 means very high similarity.
    private const double SIMILARITY_THRESHOLD = 0.6;
    private readonly ILogger<NewsDeduplicationService> _logger;

    public NewsDeduplicationService(ILogger<NewsDeduplicationService> logger)
    {
        _logger = logger;
    }

    public bool IsDuplicate(BaseNews candidate, List<NewsArticle> existingArticles)
    {
        if (candidate == null || string.IsNullOrWhiteSpace(candidate.Title))
        {
            return false;
        }

        var candidateTitleNormalized = Normalize(candidate.Title);
        var candidateUrl = candidate.Url?.Trim();

        foreach (var existing in existingArticles)
        {
            // 1. Check for exact URL match (already handled by ID, but good as a fallback)
            // If URL is identical, it's definitely the same article source
            if (!string.IsNullOrEmpty(candidateUrl) && 
                string.Equals(candidateUrl, existing.Url, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 2. Fuzzy Title Match
            var existingTitleNormalized = Normalize(existing.Title);
            
            // Optimization: If normalized titles are identical, it's a duplicate
            if (candidateTitleNormalized == existingTitleNormalized)
            {
                _logger.LogInformation("Duplicate detected (Exact Normalized Title): '{New}' vs '{Existing}'", candidate.Title, existing.Title);
                return true;
            }

            var similarity = JaccardSimilarity(candidateTitleNormalized, existingTitleNormalized);
            if (similarity >= SIMILARITY_THRESHOLD)
            {
                _logger.LogInformation("Duplicate detected (Similarity {Score:F2}): '{New}' vs '{Existing}'", similarity, candidate.Title, existing.Title);
                return true;
            }
        }

        return false;
    }

    private static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // Lowercase
        var normalized = input.ToLowerInvariant();

        // Keep only letters and digits (remove punctuation, special chars like emojis)
        var chars = normalized.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray();
        return new string(chars);
    }

    private static double JaccardSimilarity(string s1, string s2)
    {
        var set1 = GetShingles(s1);
        var set2 = GetShingles(s2);

        if (set1.Count == 0 && set2.Count == 0) return 1.0;
        if (set1.Count == 0 || set2.Count == 0) return 0.0;

        var intersection = new HashSet<string>(set1);
        intersection.IntersectWith(set2);

        var union = new HashSet<string>(set1);
        union.UnionWith(set2);

        return (double)intersection.Count / union.Count;
    }

    /// <summary>
    /// Splits text into word-based shingles (tokens).
    /// </summary>
    private static HashSet<string> GetShingles(string text)
    {
        // Split by whitespace to get words
        var words = text.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        return new HashSet<string>(words);
    }
}

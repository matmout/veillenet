using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeilleNet.Models.Entities;

[Table("ai_summaries", Schema = "containsharp")]
public class AiSummaryEntity : IHasTimestamps
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    [Column("url")]
    public string Url { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("source")]
    public string Source { get; set; } = string.Empty;

    [Column("published_date")]
    public DateTime PublishedDate { get; set; }

    [Required]
    [Column("summary")]
    public string Summary { get; set; } = string.Empty;

    [Column("ai_generated")]
    public bool AiGenerated { get; set; } = true;

    [Column("summary_date")]
    public DateTime SummaryDate { get; set; } = DateTime.UtcNow;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key to NewsArticle (optional)
    [Column("news_article_id")]
    public int? NewsArticleId { get; set; }

    [ForeignKey("NewsArticleId")]
    public virtual NewsArticle? NewsArticle { get; set; }

    // Helper method to create from AiContentSummary
    public static AiSummaryEntity FromAiContentSummary(AiContentSummary summary, int? newsArticleId = null)
    {
        return new AiSummaryEntity
        {
            Title = summary.Title,
            Url = summary.Url,
            Source = summary.Source,
            PublishedDate = summary.PublishedDate,
            Summary = summary.Summary,
            AiGenerated = summary.AiGenerated,
            SummaryDate = summary.SummaryDate,
            NewsArticleId = newsArticleId
        };
    }

    // Helper method to convert to AiContentSummary
    public AiContentSummary ToAiContentSummary()
    {
        return new AiContentSummary
        {
            Title = Title,
            Url = Url,
            Source = Source,
            PublishedDate = PublishedDate,
            Summary = Summary,
            AiGenerated = AiGenerated,
            SummaryDate = SummaryDate
        };
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeilleNet.Models.Entities;

[Table("daily_briefings", Schema = "containsharp")]
public class DailyBriefingEntity : IHasTimestamps
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("briefing_date")]
    public DateOnly BriefingDate { get; set; }

    [Required]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("article_count")]
    public int ArticleCount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static DailyBriefingEntity Create(DateOnly briefingDate, string content, int articleCount)
    {
        return new DailyBriefingEntity
        {
            BriefingDate = briefingDate,
            Content = content,
            ArticleCount = articleCount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

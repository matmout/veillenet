using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeilleNet.Models.Entities;

[Table("news_articles", Schema = "containsharp")]
public class NewsArticle
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

    [Column("summary")]
    public string Summary { get; set; } = string.Empty;

    [Column("published_date")]
    public DateTime PublishedDate { get; set; }

    [MaxLength(200)]
    [Column("author")]
    public string Author { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("source")]
    public string Source { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Column("image")]
    public string Image { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual AiSummaryEntity? AiSummary { get; set; }
    public virtual ICollection<NamedEntity> Entities { get; set; } = new List<NamedEntity>();

    // Helper method to create from BaseNews
    public static NewsArticle FromBaseNews(BaseNews news)
    {
        return new NewsArticle
        {
            Title = news.Title,
            Url = news.Url,
            Summary = news.Summary,
            PublishedDate = news.PublishedDate,
            Author = news.Author,
            Source = news.Source,
            Category = news.Category,
            Image = news.Image
        };
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeilleNet.Models.Entities;

[Table("dominant_themes", Schema = "containsharp")]
public class DominantTheme
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("generation_date")]
    public DateOnly GenerationDate { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("theme")]
    public string Theme { get; set; } = string.Empty;

    [Column("rationale")]
    public string? Rationale { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static DominantTheme Create(DateOnly generationDate, string theme, string? rationale)
    {
        return new DominantTheme
        {
            GenerationDate = generationDate,
            Theme = theme,
            Rationale = rationale,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

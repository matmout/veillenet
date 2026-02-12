using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeilleNet.Models.Entities;

[Table("named_entities", Schema = "containsharp")]
public class NamedEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for many-to-many relationship
    public virtual ICollection<NewsArticle> Articles { get; set; } = new List<NewsArticle>();
}

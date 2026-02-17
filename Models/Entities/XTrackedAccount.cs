using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeilleNet.Models.Entities;

[Table("x_tracked_accounts")]
public class XTrackedAccount
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("account_id")]
    public string AccountId { get; set; } = string.Empty;

    [Column("profile_image_url")]
    [MaxLength(500)]
    public string ProfileImageUrl { get; set; } = string.Empty;

    [Column("last_updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

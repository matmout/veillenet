using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeilleNet.Models.Entities;

[Table("newsletter_subscribers", Schema = "containsharp")]
public class NewsletterSubscriber
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("subscribed_at")]
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    [Column("unsubscribed_at")]
    public DateTime? UnsubscribedAt { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [MaxLength(100)]
    [Column("source")]
    public string Source { get; set; } = "Website"; // Website, API, Import, etc.

    [MaxLength(500)]
    [Column("unsubscribe_reason")]
    public string? UnsubscribeReason { get; set; }

    [Column("email_sent_count")]
    public int EmailSentCount { get; set; } = 0;

    [Column("last_email_sent_at")]
    public DateTime? LastEmailSentAt { get; set; }

    [MaxLength(128)]
    [Column("unsubscribe_token")]
    public string? UnsubscribeToken { get; set; }

    [Column("unsubscribe_token_expires_at")]
    public DateTime? UnsubscribeTokenExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Unsubscribe from newsletter
    /// </summary>
    public void Unsubscribe(string? reason = null)
    {
        IsActive = false;
        UnsubscribedAt = DateTime.UtcNow;
        UnsubscribeReason = reason;
        UnsubscribeToken = null; // Clear token after use
        UnsubscribeTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Resubscribe to newsletter
    /// </summary>
    public void Resubscribe()
    {
        IsActive = true;
        UnsubscribedAt = null;
        UnsubscribeReason = null;
        UnsubscribeToken = null;
        UnsubscribeTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Increment email sent counter
    /// </summary>
    public void IncrementEmailSent()
    {
        EmailSentCount++;
        LastEmailSentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Generate unsubscribe token (128 characters)
    /// </summary>
    public void GenerateUnsubscribeToken()
    {
        // Generate 128-character secure token
        var bytes = new byte[64]; // 64 bytes = 128 hex characters
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        UnsubscribeToken = Convert.ToHexString(bytes).ToLower();
        UnsubscribeTokenExpiresAt = DateTime.UtcNow.AddHours(24); // Token valid for 24h
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if unsubscribe token is valid
    /// </summary>
    public bool IsUnsubscribeTokenValid(string token)
    {
        if (string.IsNullOrEmpty(UnsubscribeToken) || string.IsNullOrEmpty(token))
            return false;

        if (UnsubscribeTokenExpiresAt == null || UnsubscribeTokenExpiresAt < DateTime.UtcNow)
            return false;

        return UnsubscribeToken.Equals(token, StringComparison.OrdinalIgnoreCase);
    }
}

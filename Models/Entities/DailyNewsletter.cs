using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeilleNet.Models.Entities;

[Table("daily_newsletters", Schema = "containsharp")]
public class DailyNewsletter : IHasTimestamps
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("newsletter_date")]
    public DateOnly NewsletterDate { get; set; } // Date unique (une newsletter par jour)

    [Required]
    [Column("subject")]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [Column("html_content")]
    public string HtmlContent { get; set; } = string.Empty;

    [Required]
    [Column("text_content")]
    public string TextContent { get; set; } = string.Empty;

    [Column("summary_count")]
    public int SummaryCount { get; set; } = 0;

    [Column("recipient_count")]
    public int RecipientCount { get; set; } = 0;

    [Column("sent_at")]
    public DateTime? SentAt { get; set; }

    [Column("is_sent")]
    public bool IsSent { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Mark newsletter as sent
    /// </summary>
    public void MarkAsSent(int recipientCount)
    {
        IsSent = true;
        SentAt = DateTime.UtcNow;
        RecipientCount = recipientCount;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Get newsletter date from DateTime (Paris timezone)
    /// </summary>
    public static DateOnly GetNewsletterDateFromUtc(DateTime utcDateTime)
    {
        var parisTimeZone = TimeZoneHelper.GetParisTimeZone();
        var parisDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, parisTimeZone);
        return DateOnly.FromDateTime(parisDateTime);
    }

    /// <summary>
    /// Create newsletter for today (Paris time)
    /// </summary>
    public static DailyNewsletter CreateForToday(string subject, string htmlContent, string textContent, int summaryCount)
    {
        return new DailyNewsletter
        {
            NewsletterDate = GetNewsletterDateFromUtc(DateTime.UtcNow),
            Subject = subject,
            HtmlContent = htmlContent,
            TextContent = textContent,
            SummaryCount = summaryCount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

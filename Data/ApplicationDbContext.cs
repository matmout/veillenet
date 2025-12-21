using Microsoft.EntityFrameworkCore;
using VeilleNet.Models.Entities;

namespace VeilleNet.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<NewsArticle> NewsArticles { get; set; }
    public DbSet<AiSummaryEntity> AiSummaries { get; set; }
    public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; }
    public DbSet<DailyNewsletter> DailyNewsletters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure NewsArticle
        modelBuilder.Entity<NewsArticle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Url).IsUnique();
            entity.HasIndex(e => e.PublishedDate);
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => e.Category);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure AiSummaryEntity
        modelBuilder.Entity<AiSummaryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Url).IsUnique();
            entity.HasIndex(e => e.SummaryDate);
            entity.HasIndex(e => e.PublishedDate);
            entity.HasIndex(e => e.Source);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure relationship with NewsArticle
            entity.HasOne(e => e.NewsArticle)
                .WithOne(n => n.AiSummary)
                .HasForeignKey<AiSummaryEntity>(e => e.NewsArticleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure NewsletterSubscriber
        modelBuilder.Entity<NewsletterSubscriber>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.SubscribedAt);
            entity.HasIndex(e => e.UnsubscribeToken); // Index for token lookup

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure DailyNewsletter
        modelBuilder.Entity<DailyNewsletter>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // CONTRAINTE UNIQUE : Une seule newsletter par jour
            entity.HasIndex(e => e.NewsletterDate)
                .IsUnique()
                .HasDatabaseName("idx_daily_newsletter_unique_date");

            entity.HasIndex(e => e.IsSent);
            entity.HasIndex(e => e.SentAt);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update UpdatedAt timestamp and ensure all DateTime are UTC
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // Ensure all DateTime properties are UTC
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType == typeof(DateTime) && property.CurrentValue != null)
                {
                    var dateTime = (DateTime)property.CurrentValue;
                    if (dateTime.Kind != DateTimeKind.Utc)
                    {
                        if (dateTime.Kind == DateTimeKind.Unspecified)
                        {
                            // Assume Paris time (Romance Standard Time on Windows)
                            try
                            {
                                var parisTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
                                property.CurrentValue = TimeZoneInfo.ConvertTimeToUtc(dateTime, parisTimeZone);
                            }
                            catch
                            {
                                // Fallback: treat as UTC
                                property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                            }
                        }
                        else // DateTimeKind.Local
                        {
                            property.CurrentValue = dateTime.ToUniversalTime();
                        }
                    }
                }
            }

            // Update UpdatedAt timestamp
            if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is NewsArticle newsArticle)
                {
                    newsArticle.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is AiSummaryEntity aiSummary)
                {
                    aiSummary.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

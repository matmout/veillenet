using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VeilleNet.Data;
using VeilleNet.Models.Entities;

namespace VeilleNet.Tools;

/// <summary>
/// Test utility to verify database connection and basic operations
/// </summary>
public static class DatabaseHealthCheck
{
    public static async Task<bool> TestConnectionAsync(ApplicationDbContext context)
    {
        try
        {
            // Test basic connection
            var canConnect = await context.Database.CanConnectAsync();
            Console.WriteLine($"Can connect to database: {canConnect}");

            if (!canConnect)
            {
                return false;
            }

            // Test query
            var count = await context.NewsArticles.CountAsync();
            Console.WriteLine($"News articles count: {count}");

            count = await context.AiSummaries.CountAsync();
            Console.WriteLine($"AI summaries count: {count}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database health check failed: {ex.Message}");
            return false;
        }
    }

    public static async Task EnsureDatabaseCreatedAsync(ApplicationDbContext context)
    {
        try
        {
            // Apply pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                Console.WriteLine($"Applying {pendingMigrations.Count()} pending migrations...");
                await context.Database.MigrateAsync();
                Console.WriteLine("Migrations applied successfully!");
            }
            else
            {
                Console.WriteLine("No pending migrations.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying migrations: {ex.Message}");
            throw;
        }
    }
}

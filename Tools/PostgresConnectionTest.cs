using Npgsql;

namespace VeilleNet.Tools;

/// <summary>
/// Simple test to verify PostgreSQL connection
/// </summary>
public static class PostgresConnectionTest
{
    public static async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            Console.WriteLine($"Testing connection with: {MaskPassword(connectionString)}");
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            Console.WriteLine("? Connection successful!");
            
            using var command = new NpgsqlCommand("SELECT version();", connection);
            var version = await command.ExecuteScalarAsync();
            Console.WriteLine($"PostgreSQL version: {version}");
            
            await connection.CloseAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Connection failed: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    private static string MaskPassword(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        if (!string.IsNullOrEmpty(builder.Password))
        {
            builder.Password = "****";
        }
        return builder.ToString();
    }
}

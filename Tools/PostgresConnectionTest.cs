using Npgsql;

namespace VeilleNet.Tools;

public static class PostgresConnectionTest
{
    public static async Task<bool> TestConnectionAsync(string connectionString, bool verbose = false)
    {
        try
        {
            if (verbose)
            {
                Console.WriteLine($"Testing connection with: {MaskPassword(connectionString)}");
            }

            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            if (verbose)
            {
                Console.WriteLine("Connection successful!");

                using var command = new NpgsqlCommand("SELECT version();", connection);
                var version = await command.ExecuteScalarAsync();
                Console.WriteLine($"PostgreSQL version: {version}");
            }

            await connection.CloseAsync();
            return true;
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
            }

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

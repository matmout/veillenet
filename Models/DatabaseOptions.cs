namespace VeilleNet.Models;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = false;
    public int MaxRetryCount { get; set; } = 3;
    public int MaxRetryDelay { get; set; } = 30;
}

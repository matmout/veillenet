namespace VeilleNet.Models.Entities;

public enum JobExecutionStatus
{
    Succeeded = 0,
    Failed = 1
}

public sealed class JobExecutionLog
{
    public int Id { get; set; }

    public string JobName { get; set; } = string.Empty;

    public string? TriggerName { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public JobExecutionStatus Status { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ErrorStackTrace { get; set; }

    public int? DurationMs { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

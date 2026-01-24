using Microsoft.EntityFrameworkCore;
using VeilleNet.Data;
using VeilleNet.Models.Entities;

namespace VeilleNet.Services.Tools;

public interface IJobExecutionLogger
{
    Task<int> StartAsync(string jobName, string? triggerName, CancellationToken cancellationToken);
    Task CompleteSuccessAsync(int executionId, CancellationToken cancellationToken);
    Task CompleteFailureAsync(int executionId, Exception exception, CancellationToken cancellationToken);
}

public sealed class JobExecutionLogger : IJobExecutionLogger
{
    private readonly ApplicationDbContext _db;

    public JobExecutionLogger(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<int> StartAsync(string jobName, string? triggerName, CancellationToken cancellationToken)
    {
        var startedAt = DateTime.UtcNow;

        var log = new JobExecutionLog
        {
            JobName = jobName,
            TriggerName = triggerName,
            StartedAt = startedAt,
            Status = JobExecutionStatus.Succeeded
        };

        _db.JobExecutionLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
        return log.Id;
    }

    public async Task CompleteSuccessAsync(int executionId, CancellationToken cancellationToken)
    {
        var log = await _db.JobExecutionLogs.FirstOrDefaultAsync(x => x.Id == executionId, cancellationToken);
        if (log == null)
        {
            return;
        }

        log.FinishedAt = DateTime.UtcNow;
        log.Status = JobExecutionStatus.Succeeded;
        log.DurationMs = (int)Math.Max(0, (log.FinishedAt.Value - log.StartedAt).TotalMilliseconds);

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteFailureAsync(int executionId, Exception exception, CancellationToken cancellationToken)
    {
        var log = await _db.JobExecutionLogs.FirstOrDefaultAsync(x => x.Id == executionId, cancellationToken);
        if (log == null)
        {
            return;
        }

        log.FinishedAt = DateTime.UtcNow;
        log.Status = JobExecutionStatus.Failed;
        log.DurationMs = (int)Math.Max(0, (log.FinishedAt.Value - log.StartedAt).TotalMilliseconds);
        log.ErrorMessage = exception.Message;
        log.ErrorStackTrace = exception.ToString();

        await _db.SaveChangesAsync(cancellationToken);
    }
}

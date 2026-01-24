using Quartz;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.Agent;

public abstract class LoggedJobBase : IJob
{
    private readonly IJobExecutionLogger _executionLogger;

    protected LoggedJobBase(IJobExecutionLogger executionLogger)
    {
        _executionLogger = executionLogger;
    }

    protected abstract Task ExecuteCoreAsync(IJobExecutionContext context);

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        var jobName = context.JobDetail.Key.Name;
        var triggerName = context.Trigger.Key.Name;

        var executionId = await _executionLogger.StartAsync(jobName, triggerName, ct);

        try
        {
            await ExecuteCoreAsync(context);
            await _executionLogger.CompleteSuccessAsync(executionId, ct);
        }
        catch (OperationCanceledException)
        {
            await _executionLogger.CompleteFailureAsync(executionId, new TaskCanceledException("Job canceled"), ct);
            throw;
        }
        catch (Exception ex)
        {
            await _executionLogger.CompleteFailureAsync(executionId, ex, ct);
            throw;
        }
    }
}

using Quartz;

namespace VeilleNet.Services.Agent;

[DisallowConcurrentExecution]
public sealed class DailyBriefingJob : IJob
{
    private readonly IDailyBriefingService _dailyBriefingService;
    private readonly ILogger<DailyBriefingJob> _logger;

    public DailyBriefingJob(
        IDailyBriefingService dailyBriefingService,
        ILogger<DailyBriefingJob> logger)
    {
        _dailyBriefingService = dailyBriefingService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        _logger.LogInformation("Daily briefing job started.");

        try
        {
            var briefing = await _dailyBriefingService.GetOrGenerateDailyBriefingAsync(ct);
            if (briefing is not null)
            {
                _logger.LogInformation("Daily briefing job completed successfully.");
            }
            else
            {
                _logger.LogWarning("Daily briefing job completed but no content was generated.");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Daily briefing job cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in daily briefing job.");
            throw;
        }
    }
}

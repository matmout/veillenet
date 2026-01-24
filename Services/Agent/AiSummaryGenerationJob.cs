using Quartz;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.Agent;

[DisallowConcurrentExecution]
public sealed class AiSummaryGenerationJob : LoggedJobBase
{
    private readonly IAiSummarizationService _aiSummarizationService;
    private readonly ILogger<AiSummaryGenerationJob> _logger;

    public AiSummaryGenerationJob(
        IJobExecutionLogger executionLogger,
        IAiSummarizationService aiSummarizationService,
        ILogger<AiSummaryGenerationJob> logger)
        : base(executionLogger)
    {
        _aiSummarizationService = aiSummarizationService;
        _logger = logger;
    }

    protected override async Task ExecuteCoreAsync(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;

        var summaries = await _aiSummarizationService.GetLatestBlogSummariesAsync(10, ct);
        var generatedCount = summaries.Count(s => s.AiGenerated);
        _logger.LogInformation("AI summary generation completed. Generated: {Count}", generatedCount);
    }
}

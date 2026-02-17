using Quartz;
using VeilleNet.Models.Entities;
using VeilleNet.Services.Agent;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.Tools;

[DisallowConcurrentExecution]
public sealed class NewsletterSendJob : IJob
{
    private readonly IAiSummarizationService _aiSummarizationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<NewsletterSendJob> _logger;

    public NewsletterSendJob(
        IAiSummarizationService aiSummarizationService,
        IEmailService emailService,
        ILogger<NewsletterSendJob> logger)
    {
        _aiSummarizationService = aiSummarizationService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;

        try
        {
            var today = DailyNewsletter.GetNewsletterDateFromUtc(DateTime.UtcNow);
            _logger.LogInformation("Newsletter send job started for {Date}", today);

            var summaries = await _aiSummarizationService.GetLatestBlogSummariesFromDatabaseAsync(10, ct);

            // Filter summaries to max 5 items while maximizing distinct sources
            var filteredSummaries = SummaryFilter.SelectMaxDistinctSources(summaries, 5);

            string? defaultTheme = await _aiSummarizationService.GetDominantThemeFromRecentNewsAsync();



            await _emailService.SendDailySummaryEmailAsync(summaries);

            _logger.LogInformation("Newsletter send job completed for {Date}", today);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Newsletter send job canceled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending newsletter");
            throw;
        }
    }
}

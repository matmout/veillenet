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
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsletterSendJob> _logger;

    public NewsletterSendJob(
        IAiSummarizationService aiSummarizationService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<NewsletterSendJob> logger)
    {
        _aiSummarizationService = aiSummarizationService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;

        // Interrupteur : "Newsletter:SendEnabled" = false dans appsettings
        // (ou variable d'env Newsletter__SendEnabled=false) pour couper l'envoi.
        if (!_configuration.GetValue("Newsletter:SendEnabled", true))
        {
            _logger.LogInformation("Newsletter send job skipped: Newsletter:SendEnabled is false");
            return;
        }

        try
        {
            var today = DailyNewsletter.GetNewsletterDateFromUtc(DateTime.UtcNow);
            _logger.LogInformation("Newsletter send job started for {Date}", today);

            var summaries = await _aiSummarizationService.GetLatestBlogSummariesFromDatabaseAsync(10, ct);

            // Filter summaries to max 5 items while maximizing distinct sources
            var filteredSummaries = SummaryFilter.SelectMaxDistinctSources(summaries, 5);

            await _emailService.SendDailySummaryEmailAsync(filteredSummaries);

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

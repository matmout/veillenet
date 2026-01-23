using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using VeilleNet.Models;
using VeilleNet.Services.News;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.Agent;

public class AiSummarizationBackgroundService : BackgroundService
{
    private readonly ILogger<AiSummarizationBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _interval;

    public AiSummarizationBackgroundService(
        ILogger<AiSummarizationBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;

        // Run once per hour
        _interval = TimeSpan.FromHours(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AI Summarization Background Service started");
        //Desable for now

        // Initial delay to avoid running immediately on startup
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var aiSummarizationService = scope.ServiceProvider.GetRequiredService<IAiSummarizationService>();
               
                // Get the latest posts from the last 24 hours
                var summaries = await aiSummarizationService.GetLatestBlogSummariesAsync(10, stoppingToken);
                string? defaultTheme = await aiSummarizationService.GetDominantThemeFromRecentNewsAsync(stoppingToken);
                _logger.LogInformation("Generated {SummaryCount} summaries for this batch", 
                    summaries.Count(s => s.AiGenerated));
                    
                // Send daily summary email to newsletter subscribers
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await emailService.SendDailySummaryEmailAsync(summaries);
                
                _logger.LogInformation("Daily AI summarization process completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AI summarization background service");
            }

            // Wait for the next interval
            await Task.Delay(_interval, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AI Summarization Background Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
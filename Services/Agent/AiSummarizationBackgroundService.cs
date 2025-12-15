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
    private readonly MistralOptions _options;
    private readonly TimeSpan _interval;
    private readonly string _recipientEmail;

    public AiSummarizationBackgroundService(
        ILogger<AiSummarizationBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<MistralOptions> options,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
        _recipientEmail = "matthieu.trachsel@gmail.com"; // configuration["EmailSettings:SourceEmail"] ?? 

        // Run once per day (24 hours)
        _interval = TimeSpan.FromHours(24);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AI Summarization Background Service started");
        //Desable for now
        if (true)
        { return; }
        // Initial delay to avoid running immediately on startup
        await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var aiSummarizationService = scope.ServiceProvider.GetRequiredService<IAiSummarizationService>();
               
                // Get the latest posts from the last 24 hours
                var summaries = await aiSummarizationService.GetLatestBlogSummariesAsync(10, stoppingToken);
                
                _logger.LogInformation("Generated {SummaryCount} summaries for this batch", 
                    summaries.Count(s => s.AiGenerated));
                    
                // Send email with results
                await SendResultsByEmailAsync(summaries, stoppingToken);
                
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

    private async Task SendResultsByEmailAsync(IEnumerable<AiContentSummary> summaries, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            
            var subject = $"ContainSharp - {DateTime.UtcNow.DayOfWeek.ToString()} AI Briefing {DateTime.UtcNow.ToString("yyyy-MM-dd")}";
            var body = BuildEmailBody(summaries);
            
            _logger.LogInformation("Sending email with summarization results to {RecipientEmail}", _recipientEmail);
            await emailService.SendEmailAsync(subject, body, _recipientEmail);
            _logger.LogInformation("Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with summarization results");
            // Don't throw to avoid breaking the background service
        }
    }
    
    private string BuildEmailBody(IEnumerable<AiContentSummary> summaries)
    {
        var summaryList = summaries.ToList();
        var encoder = HtmlEncoder.Default;
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">\n<head>");
        sb.AppendLine("    <meta charset=\"utf-8\" />");
        sb.AppendLine($"    <title>ContainSharp - {DateTime.UtcNow.DayOfWeek.ToString()} Dev Stream {DateTime.UtcNow.ToString("yyyy-MM-dd")}</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        body { font-family: 'Segoe UI', 'Segoe WPC', 'Helvetica Neue', Arial, sans-serif; background-color: #1e1e1e; color: #dcdcdc; margin: 0; padding: 0; }");
        sb.AppendLine("        a { color: #4FC1FF; text-decoration: none; }");
        sb.AppendLine("        a:hover { text-decoration: underline; }");
        sb.AppendLine("        .container { max-width: 720px; margin: 0 auto; padding: 32px 24px; }");
        sb.AppendLine("        .panel { background: #252526; border: 1px solid #3c3c3c; border-radius: 6px; box-shadow: 0 0 32px rgba(0,0,0,0.35); }");
        sb.AppendLine("        .header { padding: 24px; border-bottom: 1px solid #3c3c3c; background: linear-gradient(120deg, #007acc, #68217a); color: #fff; border-radius: 6px 6px 0 0; }");
        sb.AppendLine("        .header h1 { margin: 0; font-size: 24px; }");
        sb.AppendLine("        .header p { margin: 4px 0 0; font-size: 14px; opacity: 0.9; }");
        sb.AppendLine("        .body { padding: 24px; }");
        sb.AppendLine("        .meta { font-size: 13px; color: #9cdcfe; margin-bottom: 16px; }");
        sb.AppendLine("        .summary-card { background: #1b1b1c; border: 1px solid #333; border-radius: 6px; padding: 18px 20px; margin-bottom: 18px; }");
        sb.AppendLine("        .summary-card h3 { margin: 0 0 8px; font-size: 18px; color: #c5e1ff; }");
        sb.AppendLine("        .summary-meta { font-size: 12px; color: #a6a6a6; margin-bottom: 10px; letter-spacing: 0.4px; }");
        sb.AppendLine("        .summary-body { font-size: 14px; line-height: 1.5; color: #dcdcdc; }");
        sb.AppendLine("        .summary-body ul { padding-left: 20px; margin: 0; }");
        sb.AppendLine("        .summary-body li { margin-bottom: 6px; }");
        sb.AppendLine("        .empty { color: #b5cea8; font-style: italic; }");
        sb.AppendLine("        .footer { font-size: 12px; color: #808080; text-align: center; padding: 16px 0 0; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>\n<body>\n<div class=\"container\">\n    <div class=\"panel\">\n");
        sb.AppendLine("        <div class=\"header\">");
        sb.AppendLine("            <h1>Contain<span style=\"color:#4FC1FF\">('</span><img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAAAQCAYAAAB3cLZPAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAABdElEQVR42u2Wy2rCMBiGv0JYV7j0dVjM3GfC2ySgWcQb1cBzJYwD2o7m3YI0GxI3w3sIhWm1h5mHfVwC4H+QKQvQmJYw3X2M9p8sYQv2YwQ2sVfVv9x5NwCprm3yqB8yI9mPjYQkCk7TqkEw5RpVvI9mF1A1Vb4y5k4CwCkYwqgOeTq2pQhKqgq2GmWw2i8zE1lUjz8pI7oQyJH+5y2XJH9D9g5o6zV2f5rDkzVJk1wQ7VtA8mOeYp4i6Tq2oWwA0j7Qw8eB6Ew8mQpGkCkGgEwMZVv3j4yGkYIYHh4fGQYbJZ5QfCwGg2v1oEoVbFZVYZvT2uM7u2WkslEJvYV4dH3fVjvWkWwV1Xz+VmxqJgqLwWgQwGg0GgqIoKqoq6rqmZr5pKZpU2bP8IYhD5ZpP3f9V6p9ePp2Z2YkA1G2CwCwCw7u7u+Pj49dQWw1j8fHxwcHBwZp+5GQb+g0EoNfr9fDg8PJycnKc7i3b6oZqvQ0EoJfLy8sLCwqKiop3XdrstbYvA+2N7vQ1VVXW5ubpKSkpCQkPj6+rqmpqf3Q2rWmYJgP8mJgYHh0dDQkAAoFAoH0q7oCwBvJH2G6V6n0Wg02k0uZmZmYcHBwQh8TnQd1sFxcXJ1VVVf8BvXWcJvY1oAAAAAElFTkSuQmCC\" alt=\"ContainSharp\" style=\"vertical-align:middle;height:32px;margin:0 4px;\" /><span style=\"color:#4FC1FF\">')</span> · "+ DateTime.UtcNow.DayOfWeek.ToString() + " Dev Stream</h1>");
        sb.AppendLine($"            <p>{DateTime.UtcNow:dddd, dd MMM yyyy · HH:mm} UTC</p>");
        sb.AppendLine("        </div>\n        <div class=\"body\">");
        sb.AppendLine($"            <div class=\"meta\">Summaries generated: {summaryList.Count}</div>");

        if (summaryList.Count == 0)
        {
            sb.AppendLine("            <p class=\"empty\">No new insights were produced in this batch.</p>");
        }
        else
        {
            foreach (var summary in summaryList)
            {
                var safeTitle = encoder.Encode(summary.Title);
                var safeSource = encoder.Encode(summary.Source);
                var safeUrl = encoder.Encode(summary.Url);
                var rawHtmlSummary = string.IsNullOrWhiteSpace(summary.Summary)
                    ? "No AI summary available."
                    : summary.Summary; // déjà transformé en HTML par AddHtmlToText

                sb.AppendLine("            <article class=\"summary-card\">");
                sb.AppendLine($"                <div class=\"summary-meta\">{safeSource} · {summary.PublishedDate:yyyy-MM-dd HH:mm} UTC</div>");
                sb.AppendLine($"                <h3><a href=\"{safeUrl}\" target=\"_blank\" rel=\"noopener\">{safeTitle}</a></h3>");
                sb.AppendLine($"                <div class=\"summary-body\">{rawHtmlSummary}</div>");
                sb.AppendLine("            </article>");
            }
        }   

        sb.AppendLine("        </div>\n    </div>");
        sb.AppendLine("    <div class=\"footer\">");
        sb.AppendLine("        <p style=\"margin:0 0 8px;\">ContainSharp · Your Daily Dev Stream</p>"); 
        sb.AppendLine("        <p style=\"margin:0;font-size:11px;color:#666;\">Powered by AI · Delivered with ❤️ for devs</p>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</div>\n</body>\n</html>");

        return sb.ToString();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AI Summarization Background Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using VeilleNet.Models;
using VeilleNet.Models.Entities;
using VeilleNet.Services.Agent;
using VeilleNet.Services.Data;

namespace VeilleNet.Services.Tools
{
    public interface IEmailService
    {
        Task<bool> SendDailySummaryEmailAsync(List<AiContentSummary> summaries);
        Task<int> GetSubscriberCountAsync();
        Task<bool> SendUnsubscribeConfirmationEmailAsync(string email, string token);
        Task<bool> SendSubscriptionNotificationEmailAsync(string subscriberEmail, string source);
    }

    public class EmailService : IEmailService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly EmailSettings _emailSettings;
        private readonly INewsRepository _newsRepository;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IAmazonSimpleEmailService sesClient,
            IOptions<EmailSettings> emailSettings,
            INewsRepository newsRepository,
            ILogger<EmailService> logger)
        {
            _sesClient = sesClient;
            _emailSettings = emailSettings.Value;
            _newsRepository = newsRepository;
            _logger = logger;
        }

        public async Task<int> GetSubscriberCountAsync()
        {
            return await _newsRepository.GetActiveSubscribersCountAsync();
        }

        public async Task<bool> SendDailySummaryEmailAsync(List<AiContentSummary> summaries)
        {
            if (summaries == null || summaries.Count == 0)
            {
                _logger.LogWarning("No summaries to send in daily email");
                return false;
            }

            try
            {
                var subscribers = await _newsRepository.GetActiveSubscribersAsync();
                var recipients = subscribers.Select(s => s.Email).ToList();

                recipients.Add("matthieu.trachsel@gmail.com");

                if (recipients.Count == 0)
                {
                    _logger.LogWarning("No active newsletter subscribers found");
                    return false;
                }

                var today = DailyNewsletter.GetNewsletterDateFromUtc(DateTime.UtcNow);
                var existingNewsletter = await _newsRepository.GetTodayNewsletterAsync();
                if (existingNewsletter != null && existingNewsletter.IsSent)
                {
                    _logger.LogWarning("Newsletter for {Date} already sent. Skipping...", today);
                    return false;
                }
                    _logger.LogInformation("Sending daily summary to {Count} subscribers", recipients.Count);

                var htmlBody = BuildHtmlEmail(summaries);
                var textBody = BuildTextEmail(summaries);
                var subject = $"Contain'Sharp Daily - {DateTime.Now:dd MMMM yyyy}";

                // Save newsletter to database BEFORE sending
                
                var newsletter = DailyNewsletter.CreateForToday(subject, htmlBody, textBody, summaries.Count);
                await _newsRepository.CreateOrUpdateNewsletterAsync(newsletter);

                _logger.LogInformation("Newsletter saved to database for {Date}", today);

                var sendRequest = new SendEmailRequest
                {
                    Source = _emailSettings.SourceEmail,
                    Destination = new Destination
                    {
                        BccAddresses = recipients // Use BCC to hide recipients from each other
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = htmlBody
                            },
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = textBody
                            }
                        }
                    }
                };

                var response = await _sesClient.SendEmailAsync(sendRequest);
                
                if (!string.IsNullOrEmpty(response.MessageId))
                {
                    _logger.LogInformation("Daily summary email sent successfully. MessageId: {MessageId}", response.MessageId);
                    
                    // Mark newsletter as sent
                    await _newsRepository.MarkNewsletterAsSentAsync(today, recipients.Count);
                    
                    // Increment email sent counter for all recipients
                    foreach (var email in recipients)
                    {
                        await _newsRepository.IncrementEmailSentAsync(email);
                    }
                    
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send daily summary email");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending daily summary email");
                return false;
            }
        }

        public async Task<bool> SendUnsubscribeConfirmationEmailAsync(string email, string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Invalid email or token for unsubscribe confirmation");
                return false;
            }

            try
            {
                var unsubscribeUrl = $"https://containsharp.com/Newsletter/ConfirmUnsubscribe?token={token}";
                
                var htmlBody = BuildUnsubscribeConfirmationEmail(email, unsubscribeUrl);
                var textBody = BuildUnsubscribeConfirmationTextEmail(email, unsubscribeUrl);
                var subject = "Contain'Sharp - Confirm Your Unsubscription";

                var sendRequest = new SendEmailRequest
                {
                    Source = _emailSettings.SourceEmail,
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { email }
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = htmlBody
                            },
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = textBody
                            }
                        }
                    }
                };

                var response = await _sesClient.SendEmailAsync(sendRequest);
                
                if (!string.IsNullOrEmpty(response.MessageId))
                {
                    _logger.LogInformation("Unsubscribe confirmation email sent to {Email}. MessageId: {MessageId}", email, response.MessageId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send unsubscribe confirmation email to {Email}", email);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending unsubscribe confirmation email to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendSubscriptionNotificationEmailAsync(string subscriberEmail, string source)
        {
            if (string.IsNullOrWhiteSpace(subscriberEmail))
            {
                _logger.LogWarning("Invalid subscriber email for notification");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_emailSettings.ReportingEmail))
            {
                _logger.LogWarning("ReportingEmail is not configured in settings");
                return false;
            }

            try
            {
                var subject = "New Contain'Sharp newsletter subscription";
                var htmlBody = BuildSubscriptionNotificationEmail(subscriberEmail, source);
                var textBody = BuildSubscriptionNotificationTextEmail(subscriberEmail, source);

                var sendRequest = new SendEmailRequest
                {
                    Source = _emailSettings.SourceEmail,
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { _emailSettings.ReportingEmail }
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = htmlBody
                            },
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = textBody
                            }
                        }
                    }
                };

                var response = await _sesClient.SendEmailAsync(sendRequest);

                if (!string.IsNullOrEmpty(response.MessageId))
                {
                    _logger.LogInformation("Subscription notification sent to {ReportingEmail} for new subscriber {Email}. MessageId: {MessageId}",
                        _emailSettings.ReportingEmail, subscriberEmail, response.MessageId);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send subscription notification");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription notification for {Email}", subscriberEmail);
                return false;
            }
        }

        private static string BuildHtmlEmail(List<AiContentSummary> summaries)
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
            sb.AppendLine("            <h1>Contain<span style=\"color:#4FC1FF\">('</span><img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAAAQCAYAAAB3cLZPAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAABdElEQVR42u2Wy2rCMBiGv0JYV7j0dVjM3GfC2ySgWcQb1cBzJYwD2o7m3YI0GxI3w3sIhWm1h5mHfVwC4H+QKQvQmJYw3X2M9p8sYQv2YwQ2sVfVv9x5NwCprm3yqB8yI9mPjYQkCk7TqkEw5RpVvI9mF1A1Vb4y5k4CwCkYwqgOeTq2pQhKqgq2GmWw2i8zE1lUjz8pI7oQyJH+5y2XJH9D9g5o6zV2f5rDkzVJk1wQ7VtA8mOeYp4i6Tq2oWwA0j7Qw8eB6Ew8mQpGkCkGgEwMZVv3j4yGkYIYHh4fGQYbJZ5QfCwGg2v1oEoVbFZVYZvT2uM7u2WkslEJvYV4dH3fVjvWkWwV1Xz+VmxqJgqLwWgQwGg0GgqIoKqoq6rqmZr5pKZpU2bP8IYhD5ZpP3f9V6p9ePp2Z2YkA1G2CwCwCw7u7u+Pj49dQWw1j8fHxwcHBwZp+5GQb+g0EoNfr9fDg8PJycnKc7i3b6oZqvQ0EoJfLy8sLCwqKiop3XdrstbYvA+2N7vQ1VVXW5ubpKSkpCQkPj6+rqmpqf3Q2rWmYJgP8mJgYHh0dDQkAAoFAoH0q7oCwBvJH2G6V6n0Wg02k0uZmZmYcHBwQh8TnQd1sFxcXJ1VVVf8BvXWcJvY1oAAAAAElFTkSuQmCC\" alt=\"ContainSharp\" style=\"vertical-align:middle;height:32px;margin:0 4px;\" /><span style=\"color:#4FC1FF\">')</span> · " + DateTime.UtcNow.DayOfWeek.ToString() + " Dev Stream</h1>");
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
            sb.AppendLine("        <p style=\"margin:8px 0 0;font-size:11px;color:#666;\"><a href=\"https://containsharp.com/Newsletter\" style=\"color:#666;\">Unsubscribe</a> (secure 2-step process)</p>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</div>\n</body>\n</html>");

            return sb.ToString();
        }

        private static string BuildTextEmail(List<AiContentSummary> summaries)
        {
            var summaryList = summaries.ToList();
            var sb = new StringBuilder();

            sb.AppendLine($"Contain'Sharp Daily - {DateTime.Now:dd MMMM yyyy}");
            sb.AppendLine();
            sb.AppendLine($"Summaries generated: {summaryList.Count}");
            sb.AppendLine();

            if (summaryList.Count == 0)
            {
                sb.AppendLine("No new insights were produced in this batch.");
            }
            else
            {
                foreach (var summary in summaryList)
                {
                    var safeTitle = summary.Title;
                    var safeSource = summary.Source;
                    var safeUrl = summary.Url;
                    var rawTextSummary = string.IsNullOrWhiteSpace(summary.Summary)
                        ? "No AI summary available."
                        : summary.Summary; // déjà transformé en texte par AddHtmlToText

                    sb.AppendLine($"* [{safeTitle}]({safeUrl}) - {safeSource}");
                    sb.AppendLine($"  {rawTextSummary}");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("---");
            sb.AppendLine("Powered by AI · Delivered with ❤️ for devs");
            sb.AppendLine();
            sb.AppendLine("Unsubscribe: https://containsharp.com/Newsletter");
            sb.AppendLine("(secure 2-step process)");

            return sb.ToString();
        }

        private static string BuildUnsubscribeConfirmationEmail(string email, string unsubscribeUrl)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"utf-8\" />");
            sb.AppendLine("    <title>Confirm Your Unsubscription - Contain'Sharp</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: 'Segoe UI', Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px; }");
            sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }");
            sb.AppendLine("        .header { background: linear-gradient(120deg, #007acc, #68217a); color: white; padding: 30px 20px; text-align: center; }");
            sb.AppendLine("        .header h1 { margin: 0; font-size: 24px; }");
            sb.AppendLine("        .content { padding: 30px 20px; }");
            sb.AppendLine("        .content p { line-height: 1.6; color: #333; }");
            sb.AppendLine("        .button { display: inline-block; background: #dc3545; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0; }");
            sb.AppendLine("        .button:hover { background: #c82333; }");
            sb.AppendLine("        .footer { background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; border-top: 1px solid #e9ecef; }");
            sb.AppendLine("        .warning { background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class=\"container\">");
            sb.AppendLine("        <div class=\"header\">");
            sb.AppendLine("            <h1>🚪 Confirm Your Unsubscription</h1>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"content\">");
            sb.AppendLine($"            <p>Hello,</p>");
            sb.AppendLine($"            <p>You have requested to unsubscribe from the Contain'Sharp newsletter for the address <strong>{email}</strong>.</p>");
            sb.AppendLine("            <div class=\"warning\">");
            sb.AppendLine("                <strong>⚠️ Warning:</strong> By clicking the button below, you will no longer receive our daily newsletters with the latest .NET and C# news.");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <p>To <strong>confirm your unsubscription</strong>, please click the button below:</p>");
            sb.AppendLine($"            <center><a href=\"{unsubscribeUrl}\" class=\"button\">✅ Confirm Unsubscription</a></center>");
            sb.AppendLine("            <p><small>This link is valid for <strong>24 hours</strong>.</small></p>");
            sb.AppendLine("            <hr style=\"border: none; border-top: 1px solid #e9ecef; margin: 30px 0;\" />");
            sb.AppendLine("            <p><strong>Changed your mind?</strong></p>");
            sb.AppendLine("            <p>If you wish to remain subscribed, simply ignore this email. No action will be taken.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"footer\">");
            sb.AppendLine("            <p>Contain'Sharp · Your Daily .NET and C# News</p>");
            sb.AppendLine($"            <p>If you did not request this unsubscription, you can safely ignore this email.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            return sb.ToString();
        }

        private static string BuildUnsubscribeConfirmationTextEmail(string email, string unsubscribeUrl)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("CONFIRM YOUR UNSUBSCRIPTION");
            sb.AppendLine("========================================");
            sb.AppendLine();
            sb.AppendLine("Hello,");
            sb.AppendLine();
            sb.AppendLine($"You have requested to unsubscribe from the Contain'Sharp newsletter for the address {email}.");
            sb.AppendLine();
            sb.AppendLine("⚠️ WARNING: By clicking the link below, you will no longer receive our daily newsletters.");
            sb.AppendLine();
            sb.AppendLine("To CONFIRM your unsubscription, click this link:");
            sb.AppendLine(unsubscribeUrl);
            sb.AppendLine();
            sb.AppendLine("This link is valid for 24 hours.");
            sb.AppendLine();
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("CHANGED YOUR MIND?");
            sb.AppendLine();
            sb.AppendLine("If you wish to remain subscribed, simply ignore this email.");
            sb.AppendLine();
            sb.AppendLine("========================================");
            sb.AppendLine("Contain'Sharp · Your Daily .NET and C# News");
            sb.AppendLine();
            sb.AppendLine("If you did not request this unsubscription, ignore this email.");
            
            return sb.ToString();
        }

        private static string BuildSubscriptionNotificationEmail(string subscriberEmail, string source)
        {
            var encoder = HtmlEncoder.Default;
            var safeEmail = encoder.Encode(subscriberEmail);
            var safeSource = encoder.Encode(source);
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";

            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"utf-8\" />");
            sb.AppendLine("    <title>New newsletter subscription - Contain'Sharp</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: 'Segoe UI', Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 20px; }");
            sb.AppendLine("        .container { max-width: 600px; margin: 0 auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }");
            sb.AppendLine("        .header { background: linear-gradient(120deg, #007acc, #68217a); color: white; padding: 30px 20px; text-align: center; }");
            sb.AppendLine("        .header h1 { margin: 0; font-size: 24px; }");
            sb.AppendLine("        .content { padding: 30px 20px; }");
            sb.AppendLine("        .content p { line-height: 1.6; color: #333; }");
            sb.AppendLine("        .info-box { background: #e7f3ff; border-left: 4px solid #007acc; padding: 15px; margin: 20px 0; }");
            sb.AppendLine("        .info-box strong { color: #007acc; }");
            sb.AppendLine("        .footer { background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; border-top: 1px solid #e9ecef; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <div class=\"container\">");
            sb.AppendLine("        <div class=\"header\">");
            sb.AppendLine("            <h1>📧 New newsletter subscription</h1>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"content\">");
            sb.AppendLine("            <p>Hello,</p>");
            sb.AppendLine("            <p>A new person has subscribed to the <strong>Contain'Sharp</strong> newsletter.</p>");
            sb.AppendLine("            <div class=\"info-box\">");
            sb.AppendLine($"                <strong>Email:</strong> {safeEmail}<br>");
            sb.AppendLine($"                <strong>Source:</strong> {safeSource}<br>");
            sb.AppendLine($"                <strong>Date:</strong> {timestamp}");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <p>This person will now receive the daily newsletters.</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <div class=\"footer\">");
            sb.AppendLine("            <p>Contain'Sharp · Automated notification</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        private static string BuildSubscriptionNotificationTextEmail(string subscriberEmail, string source)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";
            var sb = new StringBuilder();

            sb.AppendLine("NEW NEWSLETTER SUBSCRIPTION");
            sb.AppendLine("========================================");
            sb.AppendLine();
            sb.AppendLine("Hello,");
            sb.AppendLine();
            sb.AppendLine("A new person has subscribed to the Contain'Sharp newsletter.");
            sb.AppendLine();
            sb.AppendLine($"Email: {subscriberEmail}");
            sb.AppendLine($"Source: {source}");
            sb.AppendLine($"Date: {timestamp}");
            sb.AppendLine();
            sb.AppendLine("This person will now receive the daily newsletters.");
            sb.AppendLine();
            sb.AppendLine("========================================");
            sb.AppendLine("Contain'Sharp · Automated notification");

            return sb.ToString();
        }
    }

    public class EmailSettings
    {
        public string SourceEmail { get; set; } = string.Empty;
        public string AwsAccessKey { get; set; } = string.Empty;
        public string AwsSecretKey { get; set; } = string.Empty;
        public string AwsRegion { get; set; } = string.Empty;
        public string ReportingEmail { get; set; } = string.Empty;
    }
}
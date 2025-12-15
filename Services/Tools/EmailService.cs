using System;
using System.Threading.Tasks;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VeilleNet.Services.Tools
{
    public class EmailService : IEmailService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IAmazonSimpleEmailService sesClient,
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _sesClient = sesClient;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string subject, string body, string toEmail)
        {
            try
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = _emailSettings.SourceEmail,
                    Destination = new Destination
                    {
                        ToAddresses = new System.Collections.Generic.List<string> { _emailSettings.SourceEmail },// toEmail }
                        BccAddresses = new System.Collections.Generic.List<string> { toEmail }
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content(body)
                        }
                    }
                };

                var response = await _sesClient.SendEmailAsync(sendRequest);
                _logger.LogInformation("Email sent successfully to {ToEmail}. MessageId: {MessageId}", toEmail, response.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                throw;
            }
        }
    }

    public class EmailSettings
    {
        public string SourceEmail { get; set; } = string.Empty;
        public string AwsAccessKey { get; set; } = string.Empty;
        public string AwsSecretKey { get; set; } = string.Empty;
        public string AwsRegion { get; set; } = string.Empty;
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Services.Data;

namespace VeilleNet.Pages;

public class ConfirmUnsubscribeModel : PageModel
{
    private readonly INewsRepository _newsRepository;
    private readonly ILogger<ConfirmUnsubscribeModel> _logger;

    public ConfirmUnsubscribeModel(
        INewsRepository newsRepository,
        ILogger<ConfirmUnsubscribeModel> logger)
    {
        _newsRepository = newsRepository;
        _logger = logger;
    }

    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = "error"; // success or error

    public async Task OnGetAsync(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            Message = "Invalid unsubscription token.";
            MessageType = "error";
            _logger.LogWarning("ConfirmUnsubscribe called without token");
            return;
        }

        try
        {
            _logger.LogInformation("Processing unsubscribe token (first 20 chars): {TokenPrefix}", token.Length > 20 ? token.Substring(0, 20) : token);
            
            var success = await _newsRepository.UnsubscribeWithTokenAsync(token);
            
            if (success)
            {
                Message = "You have been successfully unsubscribed from the Contain'Sharp newsletter. We're sorry to see you go!";
                MessageType = "success";
                _logger.LogInformation("Successfully unsubscribed via token: {Token}", token.Substring(0, 10) + "...");
            }
            else
            {
                Message = "Invalid or expired unsubscription token. The link may have expired (valid for 24 hours). Please request a new unsubscription link.";
                MessageType = "error";
                _logger.LogWarning("Invalid or expired unsubscribe token: {Token}", token.Substring(0, 10) + "...");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token unsubscription for token: {Token}", token.Substring(0, 10) + "...");
            Message = "An error occurred while processing your request. Please try again later.";
            MessageType = "error";
        }
    }
}

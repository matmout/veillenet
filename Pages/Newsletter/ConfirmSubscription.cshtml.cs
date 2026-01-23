using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Services.Tools;

namespace VeilleNet.Pages.Newsletter;

public class ConfirmSubscriptionModel : PageModel
{
    private readonly INewsletterService _newsletterService;
    private readonly ILogger<ConfirmSubscriptionModel> _logger;

    public bool IsConfirmed { get; set; }
    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = "info";

    public ConfirmSubscriptionModel(INewsletterService newsletterService, ILogger<ConfirmSubscriptionModel> logger)
    {
        _newsletterService = newsletterService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            IsConfirmed = false;
            Message = "Invalid or missing confirmation token.";
            MessageType = "error";
            return Page();
        }

        try
        {
            var success = await _newsletterService.ConfirmSubscriptionAsync(token);

            if (success)
            {
                IsConfirmed = true;
                Message = "Your subscription has been confirmed! You will now receive our daily .NET updates. 🎉";
                MessageType = "success";
                ViewData["TriggerFireworks"] = "true";
            }
            else
            {
                IsConfirmed = false;
                Message = "This link is expired or invalid. You might have already confirmed your subscription.";
                MessageType = "error";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying subscription confirmation token: {Token}", token);
            IsConfirmed = false;
            Message = "An error occurred while confirming your subscription.";
            MessageType = "error";
        }

        return Page();
    }
}

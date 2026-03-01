using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Services.Tools;
using VeilleNet.Services.Data;
using System.ComponentModel.DataAnnotations;

namespace VeilleNet.Pages;

public class NewsletterModel : PageModel
{
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<NewsletterModel> _logger;

    public NewsletterModel(
        ISubscriberRepository subscriberRepository,
        IEmailService emailService,
        ILogger<NewsletterModel> logger)
    {
        _subscriberRepository = subscriberRepository;
        _emailService = emailService;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? MessageType { get; set; } // success, error, info

    public async Task OnGetAsync()
    {
        // Page loads - no data needed for unsubscribe-only page
        await Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostRequestUnsubscribeAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var subscriber = await _subscriberRepository.GetSubscriberByEmailAsync(Email);
            
            if (subscriber == null || !subscriber.IsActive)
            {
                Message = "This email address is not subscribed to our newsletter.";
                MessageType = "info";
                _logger.LogInformation("Unsubscribe attempt for non-existent or inactive email: {Email}", Email);
            }
            else
            {
                // Check if a valid token already exists
                var hasValidToken = await _subscriberRepository.HasValidUnsubscribeTokenAsync(Email);
                
                if (hasValidToken)
                {
                    Message = "A confirmation email was already sent to you recently. Please check your inbox and spam folder. The link is valid for 24 hours.";
                    MessageType = "info";
                    _logger.LogInformation("Unsubscribe request blocked - valid token already exists for: {Email}", Email);
                    Email = string.Empty;
                }
                else
                {
                    // Generate token (will create new one or reuse expired one)
                    var token = await _subscriberRepository.GenerateUnsubscribeTokenAsync(Email);
                    
                    // Send confirmation email
                    var emailSent = await _emailService.SendUnsubscribeConfirmationEmailAsync(Email, token);
                    
                    if (emailSent)
                    {
                        Message = "A confirmation email has been sent to you. Please click the link to finalize your unsubscription.";
                        MessageType = "success";
                        _logger.LogInformation("Unsubscribe confirmation email sent to: {Email}", Email);
                        Email = string.Empty;
                    }
                    else
                    {
                        Message = "Error sending confirmation email. Please try again.";
                        MessageType = "error";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during unsubscribe request for {Email}", Email);
            Message = "An error occurred. Please try again later.";
            MessageType = "error";
        }

        return Page();
    }
}


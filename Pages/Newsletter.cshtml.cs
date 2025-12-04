using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;

namespace VeilleNet.Pages;

public class NewsletterModel : PageModel
{
    private readonly INewsletterService _newsletterService;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public List<string> SelectedTopics { get; set; } = new();

    public bool ShowSuccess { get; set; }
    public bool ShowError { get; set; }
    public string? ErrorMessage { get; set; }

    public List<string> AvailableTopics { get; set; } = new()
    {
        ".NET Releases",
        "ASP.NET Core",
        "C# Language",
        "Blazor",
        "Entity Framework",
        "Azure",
        "GitHub Trending"
    };

    public NewsletterModel(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Email))
        {
            ShowError = true;
            ErrorMessage = "Veuillez entrer une adresse email valide.";
            return Page();
        }

        if (SelectedTopics.Count == 0)
        {
            ShowError = true;
            ErrorMessage = "Veuillez sélectionner au moins un sujet d'intérêt.";
            return Page();
        }

        var subscription = new NewsletterSubscription
        {
            Email = Email,
            Topics = SelectedTopics
        };

        var success = await _newsletterService.SubscribeAsync(subscription);

        if (success)
        {
            ShowSuccess = true;
            Email = string.Empty;
            SelectedTopics = new();
        }
        else
        {
            ShowError = true;
            ErrorMessage = "Cette adresse email est déjà inscrite.";
        }

        return Page();
    }
}

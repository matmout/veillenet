using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VeilleNet.Pages;

public class SitemapModel : PageModel
{
    public List<SitemapUrl> Urls { get; set; } = new();

    public void OnGet()
    {
        var baseUrl = "https://containsharp.com";
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Core pages
        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/",
            LastMod = today,
            ChangeFreq = "daily",
            Priority = "1.0"
        });

        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/Liens",
            LastMod = today,
            ChangeFreq = "weekly",
            Priority = "0.8"
        });

        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/Newsletter",
            LastMod = today,
            ChangeFreq = "monthly",
            Priority = "0.6"
        });

        // Utility pages
        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/Sitemap",
            LastMod = today,
            ChangeFreq = "monthly",
            Priority = "0.3"
        });

        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/Error404",
            LastMod = today,
            ChangeFreq = "yearly",
            Priority = "0.1"
        });

        // Future: Add dynamic content URLs here
        // Example: Blog posts, video pages, etc.
        // foreach (var post in blogPosts)
        // {
        //     Urls.Add(new SitemapUrl { Loc = $"{baseUrl}/Post/{post.Slug}", ... });
        // }
    }
}

public class SitemapUrl
{
    public string Loc { get; set; } = string.Empty;
    public string LastMod { get; set; } = string.Empty;
    public string ChangeFreq { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;

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
            Loc = $"{baseUrl}/About",
            LastMod = today,
            ChangeFreq = "monthly",
            Priority = "0.4"
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
            Loc = $"{baseUrl}/History",
            LastMod = today,
            ChangeFreq = "daily",
            Priority = "0.8"
        });

        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/AiSummary",
            LastMod = today,
            ChangeFreq = "daily",
            Priority = "0.7"
        });
        
        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/KnowledgeGraph",
            LastMod = today,
            ChangeFreq = "daily",
            Priority = "0.9"
        });
        
        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/Roadmap",
            LastMod = today,
            ChangeFreq = "weekly",
            Priority = "0.7"
        });
        
        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/Training",
            LastMod = today,
            ChangeFreq = "weekly",
            Priority = "0.7"
        });
        
        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/MCP",
            LastMod = today,
            ChangeFreq = "weekly",
            Priority = "0.7"
        });
        
        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/LatestLLM",
            LastMod = today,
            ChangeFreq = "daily",
            Priority = "0.8"
        });

        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/Releases",
            LastMod = today,
            ChangeFreq = "daily",
            Priority = "0.8"
        });

        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/Skills",
            LastMod = today,
            ChangeFreq = "weekly",
            Priority = "0.8"
        });

        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/Radar",
            LastMod = today,
            ChangeFreq = "weekly",
            Priority = "0.8"
        });

        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/NewsletterArchive",
            LastMod = today,
            ChangeFreq = "daily",
            Priority = "0.6"
        });

        Urls.Add(new SitemapUrl
        {
            Loc = $"{baseUrl}/Privacy",
            LastMod = today,
            ChangeFreq = "yearly",
            Priority = "0.2"
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
    }
}

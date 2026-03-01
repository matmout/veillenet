using System.Net;

namespace VeilleNet.Services;

/// <summary>
/// Validates URLs before fetching to prevent Server-Side Request Forgery (SSRF) attacks.
/// Only allows known, trusted domains and blocks private/internal network addresses.
/// </summary>
public static class UrlSafetyValidator
{
    /// <summary>
    /// Domains known to host legitimate tech articles that the AI summarizer may fetch.
    /// </summary>
    private static readonly HashSet<string> AllowedDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        // Microsoft
        "devblogs.microsoft.com",
        "learn.microsoft.com",
        "blogs.windows.com",
        "azure.microsoft.com",
        "techcommunity.microsoft.com",
        "dotnet.microsoft.com",
        "github.blog",
        "visualstudiomagazine.com",

        // GitHub
        "github.com",
        "github.io",

        // Developer blogs & news
        "medium.com",
        "dev.to",
        "towardsdatascience.com",
        "infoq.com",
        "dzone.com",
        "codeproject.com",
        "c-sharpcorner.com",
        "andrewlock.net",
        "ardalis.com",
        "khalidabuhakmeh.com",
        "jimmybogard.com",
        "haacked.com",
        "hanselman.com",
        "scotthanselman.com",
        "blog.jetbrains.com",
        "jetbrains.com",
        "devexpress.com",
        "community.devexpress.com",
        "syncfusion.com",
        "telerik.com",
        "blog.cleancoder.com",
        "martinfowler.com",
        "theregister.com",
        "arstechnica.com",
        "theverge.com",
        "wired.com",
        "zdnet.com",
        "techcrunch.com",
        "venturebeat.com",

        // AI & ML
        "openai.com",
        "anthropic.com",
        "deepmind.google",
        "ai.google",
        "huggingface.co",
        "mistral.ai",
        "blog.google",

        // Stack Overflow
        "stackoverflow.com",
        "stackexchange.com",

        // YouTube (for video descriptions)
        "youtube.com",
        "www.youtube.com",

        // .NET community
        "dotnetfoundation.org",
        "nuget.org",
        "blog.nuget.org",
    };

    /// <summary>
    /// Blocked IP ranges (private networks, loopback, link-local, metadata endpoints).
    /// </summary>
    private static readonly string[] BlockedPrefixes =
    [
        "127.", "10.", "172.16.", "172.17.", "172.18.", "172.19.",
        "172.20.", "172.21.", "172.22.", "172.23.", "172.24.", "172.25.",
        "172.26.", "172.27.", "172.28.", "172.29.", "172.30.", "172.31.",
        "192.168.", "169.254.", "0.", "fc00:", "fd00:", "fe80:", "::1"
    ];

    /// <summary>
    /// Validates that a URL is safe to fetch (not internal, uses allowed scheme, from a known domain).
    /// </summary>
    /// <returns>True if the URL is safe to fetch; false otherwise.</returns>
    public static bool IsSafeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // Only allow HTTPS (and HTTP for dev compat)
        if (uri.Scheme != "https" && uri.Scheme != "http")
            return false;

        // Block file://, ftp://, etc.
        var host = uri.Host;

        // Block IP addresses directly
        if (IPAddress.TryParse(host, out var ip))
        {
            var ipStr = ip.ToString();
            if (BlockedPrefixes.Any(prefix => ipStr.StartsWith(prefix, StringComparison.Ordinal)))
                return false;

            // Also block loopback
            if (IPAddress.IsLoopback(ip))
                return false;
        }

        // Block localhost variants
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase))
            return false;

        // Check against domain allowlist (including subdomains)
        return IsAllowedDomain(host);
    }

    /// <summary>
    /// Checks if the host matches an allowed domain or is a subdomain of one.
    /// </summary>
    private static bool IsAllowedDomain(string host)
    {
        foreach (var domain in AllowedDomains)
        {
            if (host.Equals(domain, StringComparison.OrdinalIgnoreCase))
                return true;

            // Allow subdomains (e.g., "blog.openai.com" matches "openai.com")
            if (host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns a human-readable reason why a URL was rejected, or null if it's safe.
    /// </summary>
    public static string? GetRejectionReason(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return "URL is empty or null";

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return "URL is not a valid absolute URI";

        if (uri.Scheme != "https" && uri.Scheme != "http")
            return $"Scheme '{uri.Scheme}' is not allowed (only http/https)";

        var host = uri.Host;

        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase))
            return $"Host '{host}' is a local/internal address";

        if (IPAddress.TryParse(host, out _))
            return $"Direct IP addresses are not allowed: {host}";

        if (!IsAllowedDomain(host))
            return $"Domain '{host}' is not in the allowlist";

        return null;
    }
}

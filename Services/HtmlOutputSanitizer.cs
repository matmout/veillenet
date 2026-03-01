using System.Text.RegularExpressions;

namespace VeilleNet.Services;

/// <summary>
/// Sanitizes HTML output by allowing only a safe whitelist of tags.
/// Prevents XSS when rendering AI-generated summaries with @Html.Raw().
/// </summary>
public static partial class HtmlOutputSanitizer
{
    // Tags that are safe to render in AI summaries
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "ul", "ol", "li", "strong", "b", "em", "i", "code", "pre",
        "h1", "h2", "h3", "h4", "h5", "h6", "div", "span", "blockquote", "hr"
    };

    // Attributes that are safe to keep
    private static readonly HashSet<string> AllowedAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "class"
    };

    /// <summary>
    /// Sanitizes HTML content by removing all tags except those in the allowlist.
    /// Strips all attributes except allowed ones. Removes script/style content entirely.
    /// </summary>
    public static string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        // 1. Remove <script>...</script> and <style>...</style> blocks entirely
        var result = ScriptBlockRegex().Replace(html, string.Empty);
        result = StyleBlockRegex().Replace(result, string.Empty);

        // 2. Remove event handler attributes (onclick, onerror, onload, etc.)
        result = EventHandlerRegex().Replace(result, "$1");

        // 3. Remove javascript: and data: URIs in href/src attributes
        result = JavascriptUriRegex().Replace(result, "$1\"\"");

        // 4. Process each HTML tag — keep allowed ones, strip the rest
        result = HtmlTagWithAttributesRegex().Replace(result, match =>
        {
            var isClosing = match.Groups[1].Value == "/";
            var tagName = match.Groups[2].Value;
            var attributes = match.Groups[3].Value;

            if (!AllowedTags.Contains(tagName))
            {
                return string.Empty;
            }

            // Filter attributes to only allowed ones
            var safeAttributes = string.Empty;
            if (!isClosing && !string.IsNullOrWhiteSpace(attributes))
            {
                var attrMatches = AttributeRegex().Matches(attributes);
                var safeAttrs = new List<string>();
                foreach (Match attr in attrMatches)
                {
                    var attrName = attr.Groups[1].Value;
                    if (AllowedAttributes.Contains(attrName))
                    {
                        safeAttrs.Add(attr.Value);
                    }
                }
                if (safeAttrs.Count > 0)
                {
                    safeAttributes = " " + string.Join(" ", safeAttrs);
                }
            }

            return isClosing ? $"</{tagName}>" : $"<{tagName}{safeAttributes}>";
        });

        return result;
    }

    [GeneratedRegex(@"<script[^>]*>[\s\S]*?</script>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptBlockRegex();

    [GeneratedRegex(@"<style[^>]*>[\s\S]*?</style>", RegexOptions.IgnoreCase)]
    private static partial Regex StyleBlockRegex();

    [GeneratedRegex(@"(<[^>]*?)\s+on\w+\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EventHandlerRegex();

    [GeneratedRegex(@"((?:href|src)\s*=\s*)(?:""javascript:[^""]*""|'javascript:[^']*'|""data:[^""]*""|'data:[^']*')", RegexOptions.IgnoreCase)]
    private static partial Regex JavascriptUriRegex();

    [GeneratedRegex(@"<(/?)(\w+)([^>]*)>")]
    private static partial Regex HtmlTagWithAttributesRegex();

    [GeneratedRegex(@"(\w+)\s*=\s*(?:""[^""]*""|'[^']*')")]
    private static partial Regex AttributeRegex();
}

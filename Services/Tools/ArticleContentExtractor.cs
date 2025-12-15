using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace VeilleNet.Services.Tools;

public static class ArticleContentExtractor
{
    /// <summary>
    /// Intelligently extracts the main article content from HTML using the title as a guide
    /// </summary>
    public static string ExtractArticleContent(string html, string articleTitle)
    {
        try
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Try to find the main article content using common patterns
            var mainContent = TryFindMainContent(doc);
            
            if (!string.IsNullOrWhiteSpace(mainContent))
            {
                return CleanAndOptimizeContent(mainContent, articleTitle);
            }
            
            // If no main content found, try to find content that mentions the title
            var titleRelatedContent = FindTitleRelatedContent(doc, articleTitle);
            
            if (!string.IsNullOrWhiteSpace(titleRelatedContent))
            {
                return CleanAndOptimizeContent(titleRelatedContent, articleTitle);
            }
            
            // Fallback: get all paragraph text
            var paragraphs = doc.DocumentNode.SelectNodes("//p");
            if (paragraphs != null)
            {
                var paragraphText = string.Join("\n\n", paragraphs.Select(p => p.InnerText.Trim()));
                return CleanAndOptimizeContent(paragraphText, articleTitle);
            }
            
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string TryFindMainContent(HtmlDocument doc)
    {
        // Try common main content selectors
        var selectors = new[]
        {
            "//article",
            "//main",
            "//div[@class='post-content']",
            "//div[@class='article-content']",
            "//div[@class='entry-content']",
            "//div[@itemprop='articleBody']",
            "//div[@role='main']"
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            if (node != null)
            {
                return node.InnerText.Trim();
            }
        }

        return string.Empty;
    }

    private static string FindTitleRelatedContent(HtmlDocument doc, string articleTitle)
    {
        // Look for content that contains the title or similar text
        var allTextNodes = doc.DocumentNode.SelectNodes("//text()[normalize-space()]");
        
        if (allTextNodes == null)
        {
            return string.Empty;
        }

        var titleWords = articleTitle.Split(new[] {' ', '-', '_'}, StringSplitOptions.RemoveEmptyEntries);
        
        // Find nodes that contain multiple title words
        var relevantNodes = new List<HtmlNode>();
        
        foreach (var node in allTextNodes)
        {
            var text = node.InnerText.Trim();
            var matchingWords = titleWords.Count(tw => text.Contains(tw, StringComparison.OrdinalIgnoreCase));
            
            if (matchingWords >= 2 && text.Length > 50) // At least 2 matching words and reasonable length
            {
                relevantNodes.Add(node.ParentNode);
            }
        }

        if (relevantNodes.Count > 0)
        {
            // Get the common ancestor of relevant nodes
            var commonAncestor = FindCommonAncestor(relevantNodes);
            return commonAncestor?.InnerText.Trim() ?? string.Empty;
        }

        return string.Empty;
    }

    private static HtmlNode FindCommonAncestor(List<HtmlNode> nodes)
    {
        if (nodes.Count == 0) return null;
        
        var currentAncestor = nodes[0];
        
        foreach (var node in nodes.Skip(1))
        {
            currentAncestor = FindCommonAncestor(currentAncestor, node);
            if (currentAncestor == null) break;
        }

        return currentAncestor;
    }

    private static HtmlNode FindCommonAncestor(HtmlNode node1, HtmlNode node2)
    {
        var ancestors1 = GetAncestors(node1);
        var ancestors2 = GetAncestors(node2);
        
        for (int i = 0; i < Math.Min(ancestors1.Count, ancestors2.Count); i++)
        {
            if (ancestors1[i] == ancestors2[i])
            {
                return ancestors1[i];
            }
        }

        return null;
    }

    private static List<HtmlNode> GetAncestors(HtmlNode node)
    {
        var ancestors = new List<HtmlNode>();
        var current = node;
        
        while (current != null)
        {
            ancestors.Add(current);
            current = current.ParentNode;
        }

        return ancestors;
    }

    private static string CleanAndOptimizeContent(string content, string articleTitle)
    {
        // Remove excessive whitespace
        content = Regex.Replace(content, @"\s+", " ");
        content = content.Trim();

        // Remove common boilerplate text
        content = RemoveBoilerplate(content);

        // Ensure the content is relevant to the title
        content = EnsureRelevanceToTitle(content, articleTitle);

        // Limit content length
        if (content.Length > 12000) // Max input for Mistral
        {
            // Try to find a good breaking point
            var breakPoints = new[] {". ", "! ", "? "};
            int lastBreak = -1;
            
            foreach (var breakPoint in breakPoints)
            {
                var lastIndex = content.LastIndexOf(breakPoint, Math.Min(12000, content.Length - 1));
                if (lastIndex > lastBreak)
                {
                    lastBreak = lastIndex + breakPoint.Length;
                }
            }

            if (lastBreak > 0)
            {
                content = content.Substring(0, lastBreak).Trim();
            }
            else
            {
                content = content.Substring(0, 12000);
            }
        }

        return content;
    }

    private static string RemoveBoilerplate(string content)
    {
        // Remove common boilerplate patterns
        var patterns = new[]
        {
            "\\bCopyright\\b.*$",
            "\\bAll rights reserved\\b.*$",
            "\\bPrivacy Policy\\b.*$",
            "\\bTerms of Service\\b.*$",
            "\\bCookie Policy\\b.*$",
            "\\bContact Us\\b.*$",
            "\\bAbout Us\\b.*$",
            "\\bSubscribe to our newsletter\\b.*$",
            "\\bFollow us on\\b.*$",
            "\\bShare this:\\b.*$",
            "\\bYou might also like\\b.*$",
            "\\bRelated posts:\\b.*$",
            "\\bComments\\b.*$",
            "\\bLeave a Reply\\b.*$"
        };

        foreach (var pattern in patterns)
        {
            content = Regex.Replace(content, pattern, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        // Remove multiple newlines
        content = Regex.Replace(content, @"\\n\\s*\\n\\s*\\n+", "\n\n");

        return content.Trim();
    }

    private static string EnsureRelevanceToTitle(string content, string articleTitle)
    {
        // Check if content contains enough relevant information
        var titleWords = articleTitle.Split(new[] {' ', '-', '_'}, StringSplitOptions.RemoveEmptyEntries);
        
        if (titleWords.Length == 0)
        {
            return content;
        }

        var contentWords = content.Split(new[] {' ', '.', ',', '!', '?'}, StringSplitOptions.RemoveEmptyEntries);
        
        var matchingWords = titleWords.Count(tw => 
            contentWords.Any(cw => cw.Contains(tw, StringComparison.OrdinalIgnoreCase)));
        
        // If less than 30% of title words are found, try to find better content
        if (matchingWords < titleWords.Length * 0.3)
        {
            // This might not be the right content, but we'll return it anyway
            // as a fallback since we don't have better options
        }

        return content;
    }
}
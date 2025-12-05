using System.Text.RegularExpressions;
using System.Web;

namespace VeilleNet.Services;

/// <summary>
/// Service utilitaire pour nettoyer le contenu HTML des flux RSS.
/// </summary>
public static partial class HtmlSanitizer
{
    /// <summary>
    /// Retire toutes les balises HTML du contenu et décode les entités HTML.
    /// </summary>
    /// <param name="html">Le contenu HTML à nettoyer.</param>
    /// <returns>Le texte sans balises HTML.</returns>
    public static string StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        // Retirer les balises HTML
        var text = HtmlTagRegex().Replace(html, string.Empty);

        // Décoder les entités HTML (ex: &nbsp; -> espace, &amp; -> &)
        text = HttpUtility.HtmlDecode(text);

        // Normaliser les espaces multiples
        text = MultipleSpacesRegex().Replace(text, " ");

        return text.Trim();
    }

    [GeneratedRegex("<[^>]*>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex MultipleSpacesRegex();
}

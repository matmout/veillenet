namespace VeilleNet.Models.ViewModels;

public class NewsBlockViewModel
{
    // --- Header ---
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string HeaderCssClass { get; set; } = string.Empty;
    /// <summary>Inline CSS style for header (gradients, custom backgrounds).</summary>
    public string? HeaderStyle { get; set; }
    /// <summary>Raw HTML for custom header icon (e.g., SVG). When set, <see cref="Icon"/> is ignored.</summary>
    public string? HeaderIconHtml { get; set; }
    /// <summary>Optional badge HTML displayed after the title.</summary>
    public string? HeaderBadgeHtml { get; set; }

    // --- Layout ---
    /// <summary>Max-height of the scrollable body in pixels (default 400).</summary>
    public int MaxHeight { get; set; } = 400;
    /// <summary>Number of columns per row: 1 = standard list, 2 = two-column grid.</summary>
    public int ColumnsPerRow { get; set; } = 1;

    // --- Image ---
    public string? DefaultImageUrl { get; set; }
    public string? DefaultIconClass { get; set; }
    public int ImageWidth { get; set; } = 60;
    public int ImageHeight { get; set; } = 45;

    // --- Content display ---
    /// <summary>Show item title. Set false for X Posts (body text only).</summary>
    public bool ShowTitle { get; set; } = true;
    /// <summary>Show AI summary badge link.</summary>
    public bool ShowAiSummary { get; set; } = true;
    /// <summary>HTML-sanitize summaries (true for RSS content, false for plain text).</summary>
    public bool SanitizeHtml { get; set; } = true;
    public int? SummaryMaxLength { get; set; }

    // --- Metadata ---
    public bool ShowAuthor { get; set; }
    /// <summary>Show source tag in metadata line.</summary>
    public bool ShowSource { get; set; } = true;
    /// <summary>Date format string (default "dd MMM yyyy").</summary>
    public string DateFormat { get; set; } = "dd MMM yyyy";
    /// <summary>Icon class for author metadata (default bi-person).</summary>
    public string AuthorIcon { get; set; } = "bi-person";

    // --- Section-specific rendering ---
    /// <summary>Show StackOverflow-style tags.</summary>
    public bool ShowTags { get; set; }
    /// <summary>Show GitHub-style stats (stars/forks) instead of standard metadata.</summary>
    public bool ShowStats { get; set; }

    // --- Data ---
    public string LoadingText { get; set; } = "Loading...";
    public string? EmptyStateTitle { get; set; }
    public string? EmptyStateMessage { get; set; }
    public string EmptyStateIcon { get; set; } = "bi-info-circle";
    public bool HasEmptyState => !string.IsNullOrWhiteSpace(EmptyStateTitle) || !string.IsNullOrWhiteSpace(EmptyStateMessage);
    public List<NewsBlockItem> Items { get; set; } = [];
}

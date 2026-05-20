namespace VeilleNet.Models.ViewModels;

/// <summary>
/// Universal item DTO for _NewsBlockPartial, abstracting all model types
/// (BaseNews, Video, XPost, StackOverflowQuestion, GitHubProject).
/// </summary>
public class NewsBlockItem
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public DateTime PublishedDate { get; set; }
    public string? Author { get; set; }
    public string? Source { get; set; }
    public string? Image { get; set; }
    public bool HasAiSummary { get; set; }

    // Avatar fallback (X Posts: ProfileImageUrl when no media)
    public string? AvatarUrl { get; set; }

    // Video overlay (shows play button when set)
    public bool ShowVideoOverlay { get; set; }

    // External link in metadata ("View on X", etc.)
    public string? ExternalLinkText { get; set; }
    public string? ExternalLinkUrl { get; set; }

    // Tags (StackOverflow)
    public List<string> Tags { get; set; } = [];

    // Stats (GitHub)
    public int? Stars { get; set; }
    public int? Forks { get; set; }
}

/// <summary>
/// Mapping extensions to convert domain models to NewsBlockItem.
/// </summary>
public static class NewsBlockItemMapper
{
    public static List<NewsBlockItem> ToBlockItems(this IEnumerable<BaseNews> items) =>
        items.Select(n => new NewsBlockItem
        {
            Title = n.Title,
            Url = n.Url,
            Summary = n.Summary,
            PublishedDate = n.PublishedDate,
            Author = n.Author,
            Source = n.Source,
            Image = n.Image,
            HasAiSummary = n.HasAiSummary
        }).ToList();

    public static List<NewsBlockItem> ToBlockItems(this IEnumerable<Video> items) =>
        items.Select(v => new NewsBlockItem
        {
            Title = v.Title,
            Url = v.Url,
            Summary = v.Description,
            PublishedDate = v.PublishedDate,
            Author = v.Channel,
            Image = v.Thumbnail
        }).ToList();

    public static List<NewsBlockItem> ToBlockItems(this IEnumerable<XPost> items) =>
        items.Select(p => new NewsBlockItem
        {
            Title = string.Empty, // X Posts don't display titles
            Url = p.Url,
            Summary = p.Text,
            PublishedDate = p.PublishedDate,
            Author = p.Author,
            Image = !string.IsNullOrEmpty(p.VideoPreviewImageUrl) ? p.VideoPreviewImageUrl : p.MediaUrl,
            AvatarUrl = p.ProfileImageUrl,
            ShowVideoOverlay = !string.IsNullOrEmpty(p.VideoUrl) || !string.IsNullOrEmpty(p.VideoPreviewImageUrl),
            ExternalLinkText = "View on X",
            ExternalLinkUrl = p.Url
        }).ToList();

    public static List<NewsBlockItem> ToBlockItems(this IEnumerable<RedditPost> items) =>
        items.Select(p => new NewsBlockItem
        {
            Title = p.Title,
            Url = p.Url,
            Summary = string.IsNullOrWhiteSpace(p.Text) ? null : p.Text,
            PublishedDate = p.PublishedDate,
            Author = p.Author,
            Source = p.Subreddit,
            Image = p.Thumbnail,
            ExternalLinkText = $"↑ {p.Score:N0}  💬 {p.NumComments:N0}",
            ExternalLinkUrl = p.Url
        }).ToList();

    public static List<NewsBlockItem> ToBlockItems(this IEnumerable<StackOverflowQuestion> items) =>
        items.Select(q => new NewsBlockItem
        {
            Title = q.Title,
            Url = q.Url,
            Summary = q.Summary,
            PublishedDate = q.PublishedDate,
            Author = q.Author,
            Tags = q.Tags
        }).ToList();

    public static List<NewsBlockItem> ToBlockItems(this IEnumerable<GitHubProject> items) =>
        items.Select(p => new NewsBlockItem
        {
            Title = p.FullName,
            Url = p.Url,
            Summary = p.Description,
            PublishedDate = p.UpdatedAt,
            Stars = p.Stars,
            Forks = p.Forks
        }).ToList();
}

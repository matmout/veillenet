namespace VeilleNet.Models.ViewModels;

public class NewsBlockViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string HeaderCssClass { get; set; } = string.Empty;
    public string? DefaultImageUrl { get; set; }
    public string? DefaultIconClass { get; set; }
    public bool ShowAuthor { get; set; }
    public int? SummaryMaxLength { get; set; }
    public string LoadingText { get; set; } = "Loading...";
    public List<BaseNews> Items { get; set; } = new();
}

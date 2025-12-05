namespace VeilleNet.Models;

public class WinFormNews
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g., "Microsoft", "DevExpress"
}

namespace VeilleNet.Models;

public class Video
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Thumbnail { get; set; } = string.Empty;
}

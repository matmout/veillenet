namespace VeilleNet.Models;

public class XPost
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string VideoPreviewImageUrl { get; set; } = string.Empty;
    public string VideoContentType { get; set; } = string.Empty;
    public string Source { get; set; } = "X";
}

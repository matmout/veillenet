namespace VeilleNet.Models;

public class RedditPost
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Subreddit { get; set; } = string.Empty;
    public int Score { get; set; }
    public int NumComments { get; set; }
    public DateTime PublishedDate { get; set; }
    public string? Thumbnail { get; set; }
}

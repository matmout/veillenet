namespace VeilleNet.Models;

public class RedditOptions
{
    public const string SectionName = "Reddit";

    public bool Enabled { get; set; } = true;
    public int CacheMinutes { get; set; } = 60;
    public int PostsPerSubreddit { get; set; } = 10;
}

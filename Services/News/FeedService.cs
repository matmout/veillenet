using System.ServiceModel.Syndication;
using System.Xml;
using VeilleNet.Models;

namespace VeilleNet.Services.News;

public interface IFeedService
{
    Task<List<BaseNews>> FetchNewsFeedAsync(string source, string feedUrl, string defaultImage , string category = "General", Func<BaseNews, bool>? filter = null);
    Task<List<Video>> FetchVideoFeedAsync(string channel, string feedUrl, Func<Video, bool>? filter = null);
}

public class FeedService : IFeedService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FeedService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<BaseNews>> FetchNewsFeedAsync(string source, string feedUrl, string defaultImage, string category = "General", Func<BaseNews, bool>? filter = null)
    {
        var newsItems = new List<BaseNews>();

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "VeilleNet/1.0");
            
            using var stream = await httpClient.GetStreamAsync(feedUrl);
            using var xmlReader = XmlReader.Create(stream);
            
            var feed = SyndicationFeed.Load(xmlReader);

            foreach (var item in feed.Items.Take(10))
            {
                var title = item.Title?.Text ?? "No title";
                var summary = HtmlSanitizer.StripHtml(item.Summary?.Text);
                
                // Try to extract author from creator extension
                var author = item.Authors.FirstOrDefault()?.Name;
                if (string.IsNullOrEmpty(author))
                {
                    var creatorExtension = item.ElementExtensions.FirstOrDefault(e => e.OuterName == "creator");
                    if (creatorExtension != null)
                    {
                        author = creatorExtension.GetObject<string>();
                    }
                }
                author ??= "Unknown";
                
                // Try to extract image from extension
                var image = "";
                var imageExtension = item.ElementExtensions.FirstOrDefault(e => e.OuterName == "image");
                if (imageExtension != null)
                {
                    image = imageExtension.GetObject<string>();
                }

                if (string.IsNullOrEmpty(image))
                {
                    image = defaultImage;
                }

                var newsItem = new BaseNews
                {
                    Title = title,
                    Url = item.Links.FirstOrDefault()?.Uri.ToString() ?? "",
                    Summary = summary,
                    PublishedDate = item.PublishDate.DateTime,
                    Author = author,
                    Source = source,
                    Category = category,
                    Image = image
                };
                
                // Apply filter if provided
                if (filter == null || filter(newsItem))
                {
                    newsItems.Add(newsItem);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching news feed {source} {feedUrl}: {ex.Message}", ex);
        }

        return newsItems;
    }

    public async Task<List<Video>> FetchVideoFeedAsync(string channel, string feedUrl, Func<Video, bool>? filter = null)
    {
        var videos = new List<Video>();

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "VeilleNet/1.0");
            
            using var stream = await httpClient.GetStreamAsync(feedUrl);
            using var xmlReader = XmlReader.Create(stream);
            
            var feed = SyndicationFeed.Load(xmlReader);

            foreach (var item in feed.Items.Take(5))
            {
                var title = item.Title?.Text ?? "No title";
                var description = HtmlSanitizer.StripHtml(item.Summary?.Text);
                var videoUrl = item.Links.FirstOrDefault()?.Uri.ToString() ?? "";
                
                // Extract YouTube video ID for thumbnail
                var thumbnail = "";
                if (videoUrl.Contains("youtube.com") || videoUrl.Contains("youtu.be"))
                {
                    var videoId = ExtractYouTubeVideoId(videoUrl);
                    if (!string.IsNullOrEmpty(videoId))
                    {
                        thumbnail = $"https://img.youtube.com/vi/{videoId}/mqdefault.jpg";
                    }
                }

                var video = new Video
                {
                    Title = title,
                    Url = videoUrl,
                    Description = description,
                    PublishedDate = item.PublishDate.DateTime,
                    Channel = channel,
                    Thumbnail = thumbnail
                };
                
                // Apply filter if provided
                if (filter == null || filter(video))
                {
                    videos.Add(video);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching video feed {channel} {feedUrl}: {ex.Message}", ex);
        }

        return videos;
    }

    private string ExtractYouTubeVideoId(string url)
    {
        try
        {
            if (url.Contains("youtube.com/watch?v="))
            {
                var uri = new Uri(url);
                var videoId = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("v");
                return videoId ?? "";
            }
            else if (url.Contains("youtu.be/"))
            {
                var parts = url.Split('/');
                return parts[parts.Length - 1].Split('?')[0];
            }
        }
        catch
        {
            return "";
        }

        return "";
    }
}
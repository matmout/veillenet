using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using VeilleNet.Models;

namespace VeilleNet.Services.News;

public interface IFeedService
{
    Task<List<BaseNews>> FetchNewsFeedAsync(string source, string feedUrl, string defaultImage, int newsCount, string category = "General", Func<BaseNews, bool>? filter = null);
    Task<List<Video>> FetchVideoFeedAsync(string channel, string feedUrl, Func<Video, bool>? filter = null);
}

public class FeedService : IFeedService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FeedService> _logger;

    public FeedService(IHttpClientFactory httpClientFactory, ILogger<FeedService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<BaseNews>> FetchNewsFeedAsync(string source, string feedUrl, string defaultImage,int newsCount, string category = "General", Func<BaseNews, bool>? filter = null)
    {
        var newsItems = new List<BaseNews>();

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VeilleNet/1.0");

            using var response = await httpClient.GetAsync(feedUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync();

            // Optionnel: garde-fou si le serveur renvoie HTML/anti-bot
            if (!xml.TrimStart().StartsWith("<", StringComparison.Ordinal))
                return newsItems;

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreWhitespace = true,
                IgnoreComments = true
            };

            using var stringReader = new StringReader(xml);
            using var reader = XmlReader.Create(stringReader, settings);

            reader.MoveToContent(); // important

            var feed = SyndicationFeed.Load(reader);
            if (feed == null)
                return newsItems;

            foreach (var item in feed.Items.Take(newsCount))
            {
                var title = item.Title?.Text ?? "No title";
                var summary = HtmlSanitizer.StripHtml(item.Summary?.Text);

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

                if (filter == null || filter(newsItem))
                {
                    newsItems.Add(newsItem);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching news feed {Source} {FeedUrl}", source, feedUrl);
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

                if (filter == null || filter(video))
                {
                    videos.Add(video);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching video feed {Channel} {FeedUrl}", channel, feedUrl);
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
                return parts[^1].Split('?')[0];
            }
        }
        catch
        {
            return "";
        }

        return "";
    }
}
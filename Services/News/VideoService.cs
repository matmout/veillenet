using System.ServiceModel.Syndication;
using System.Xml;
using VeilleNet.Models;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services.News;

public interface IVideoService
{
    Task<List<Video>> GetLatestVideosAsync();
}

public class VideoService : IVideoService
{
    private readonly ICacheService _cacheService;
    private readonly IFeedService _feedService;
    private const string CacheKey = "CSharpVideos";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    private readonly List<(string Name, string Url)> _videoFeeds = new()
    {
        ("dotNET YouTube", "https://www.youtube.com/feeds/videos.xml?channel_id=UCvtT19MZW8dq5Wwfu6B0oxw"),
        ("Microsoft Developer YouTube", "https://www.youtube.com/feeds/videos.xml?channel_id=UCsMica-v34Irf9KVTh6xx-g"),
        ("Visual Studio YouTube", "https://www.youtube.com/feeds/videos.xml?channel_id=UChqrDOwARrxdJF-ykAptc7w"),
        ("Nick Chapsas", "https://www.youtube.com/feeds/videos.xml?channel_id=UCrkPsvLGln62OMZRO6K-llg"),
        ("Tim Corey", "https://www.youtube.com/feeds/videos.xml?channel_id=UC-ptWR16ITQyYOglXyQmpzw"),
        ("Raw Coding", "https://www.youtube.com/feeds/videos.xml?channel_id=UCU9pX8hKcrx06XfOB-VQLdw"),
        ("Coding Militia", "https://www.youtube.com/feeds/videos.xml?channel_id=UC0dRNNjwGcx-LoWQFvBjQdg"),
        ("Milan Jovanović", "https://www.youtube.com/feeds/videos.xml?channel_id=UCYLAyOZ6J7_3XbWjeLbq_Yw"),
        ("Les Jackson", "https://www.youtube.com/feeds/videos.xml?channel_id=UCIMRGVXufHT69s1uaHHYJIA"),
        ("Traversy Media", "https://www.youtube.com/feeds/videos.xml?channel_id=UC29ju8bIPH5as8OGnQzwJyA"),
        ("Pluralsight", "https://www.youtube.com/feeds/videos.xml?channel_id=UCFgWvZUmgeYWd5LOm1hmgXQ"),
        ("freeCodeCamp.org", "https://www.youtube.com/feeds/videos.xml?channel_id=UC8butISFwT-Wl7EV0hUK0BQ"),
        ("Programming with Mosh", "https://www.youtube.com/feeds/videos.xml?channel_id=UCWv7vMbMWH4-V0ZXdmDpPBA"),
        ("Kudvenkat", "https://www.youtube.com/feeds/videos.xml?channel_id=UCCTVrRB5KpIiK6V2GGVsR1Q"),
    };

    public VideoService(ICacheService cacheService, IFeedService feedService)
    {
        _cacheService = cacheService;
        _feedService = feedService;
    }

    public async Task<List<Video>> GetLatestVideosAsync()
    {
        var cachedVideos = _cacheService.Get<List<Video>>(CacheKey);
        if (cachedVideos != null)
        {
            return cachedVideos;
        }

        var videos = new List<Video>();

        foreach (var (name, url) in _videoFeeds)
        {
            try
            {
                var feedVideos = await _feedService.FetchVideoFeedAsync(name, url, video => IsCSharpRelated(video.Title, video.Description) && !videos.Any(w => w.Title == video.Title));
                videos.AddRange(feedVideos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible de lire le contenu Video {name} {url} Erreur : {ex.Message}", ex);
                // Log error in production, continue with other feeds
            }
        }

        videos = videos.OrderByDescending(v => v.PublishedDate).Take(50).ToList();
        _cacheService.Set(CacheKey, videos, CacheExpiration);

        return videos;
    }



    private string ExtractYouTubeVideoId(string url)
    {
        try
        {
            var uri = new Uri(url);
            if (uri.Host.Contains("youtube.com"))
            {
                var queryParams = uri.Query.TrimStart('?').Split('&');
                foreach (var param in queryParams)
                {
                    var keyValue = param.Split('=');
                    if (keyValue.Length == 2 && keyValue[0] == "v")
                    {
                        return keyValue[1];
                    }
                }
                return "";
            }
            else if (uri.Host.Contains("youtu.be"))
            {
                return uri.AbsolutePath.TrimStart('/');
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'extraction de l'ID YouTube de l'URL {url} : {ex.Message}", ex);
            // Handle URL parsing errors
        }

        return "";
    }

    private bool IsCSharpRelated(string title, string description)
    {
        var keywords = new[] { "c#", "csharp", ".net", "dotnet", "asp.net", "aspnet",
                               "blazor", "maui", "xamarin", "visual studio", "azure",
                               "entity framework", "ef core", "linq", "wpf", "winforms",
                               "ai","copilot","github","codex","claude","code","server",
                               "css","angular","data","developer","agent","mcp","security" };
        
        var combinedText = $"{title} {description}".ToLowerInvariant();
        return keywords.Any(keyword => combinedText.Contains(keyword));
    }
}

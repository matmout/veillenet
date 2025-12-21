using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using VeilleNet.Models;
using VeilleNet.Services.Data;
using VeilleNet.Services.Tools;

namespace VeilleNet.Controllers;

[ApiController]
[Route("api")]
public class AiSummaryApiController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<AiSummaryApiController> _logger;
    private readonly INewsRepository _newsRepository;

    public AiSummaryApiController(INewsRepository newsRepository,ICacheService cacheService, ILogger<AiSummaryApiController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
        _newsRepository = newsRepository;
    }

    [HttpGet("ai-summary")]
    public async Task<IActionResult> GetAiSummary([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest(new { success = false, message = "URL parameter is required" });
        }

        try
        {
            var cacheKey = GetCacheKey(url);
            var cached = _cacheService.Get<AiContentSummary>(cacheKey);

            if (cached == null)
            {
                var summary = await _newsRepository.GetAiSummaryByUrlAsync(url);
                if (summary != null)
                {
                    _cacheService.Set(cacheKey, summary.ToAiContentSummary(), TimeSpan.FromHours(24));
                    cached = summary.ToAiContentSummary();
                }
                else
                {
                    return NotFound(new { success = false, message = "AI summary not found for this article" });
                }
            }

            return Ok(new
            {
                success = true,
                title = cached.Title,
                url = cached.Url,
                source = cached.Source,
                publishedDate = cached.PublishedDate,
                summary = cached.Summary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AI summary for URL: {Url}", url);
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }

    private static string GetCacheKey(string url)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return "AiSummary:" + Convert.ToHexString(hash);
    }
}

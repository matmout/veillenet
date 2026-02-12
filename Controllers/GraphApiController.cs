using Microsoft.AspNetCore.Mvc;
using VeilleNet.Services.Data;

namespace VeilleNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GraphApiController : ControllerBase
{
    private readonly INewsRepository _newsRepository;
    private readonly ILogger<GraphApiController> _logger;

    public GraphApiController(INewsRepository newsRepository, ILogger<GraphApiController> logger)
    {
        _newsRepository = newsRepository;
        _logger = logger;
    }

    [HttpGet("data")]
    public async Task<IActionResult> GetGraphData(CancellationToken cancellationToken)
    {
        try
        {
            var entities = await _newsRepository.GetEntitiesWithArticlesAsync(100, cancellationToken);
            
            var nodes = new List<object>();
            var links = new List<object>();
            var seenNodes = new HashSet<string>();

            foreach (var entity in entities)
            {
                var entityNodeId = $"e_{entity.Id}";
                if (seenNodes.Add(entityNodeId))
                {
                    nodes.Add(new
                    {
                        id = entityNodeId,
                        label = entity.Name,
                        type = "entity",
                        val = entity.Articles.Count * 2,
                        articles = entity.Articles.Select(a => new {
                            id = a.Id,
                            title = a.Title,
                            url = a.Url,
                            date = a.PublishedDate,
                            source = a.Source,
                            summary = a.AiSummary?.Summary ?? a.Summary
                        }).ToList()
                    });
                }

                foreach (var article in entity.Articles)
                {
                    var articleNodeId = $"a_{article.Id}";
                    if (seenNodes.Add(articleNodeId))
                    {
                        nodes.Add(new
                        {
                            id = articleNodeId,
                            label = article.Title,
                            type = "article",
                            date = article.PublishedDate,
                            summary = article.AiSummary?.Summary ?? article.Summary,
                            source = article.Source,
                            url = article.Url,
                            val = 5
                        });
                    }

                    links.Add(new
                    {
                        source = articleNodeId,
                        target = entityNodeId,
                        value = 1
                    });
                }
            }

            return Ok(new { nodes, links });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting graph data");
            return StatusCode(500, "Internal server error");
        }
    }
}

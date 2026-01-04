using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;

namespace VeilleNet.Pages;

public class RoadmapModel : PageModel
{
    public List<RoadmapItem> LearningPath { get; set; } = new();

    public void OnGet()
    {
        LearningPath = RoadmapHelper.GetCSharpLearningPath();
    }
}

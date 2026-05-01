using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;

namespace VeilleNet.Pages;

public class SkillsModel : PageModel
{
    private readonly ISkillsService _skillsService;

    public SkillPageData PageData { get; private set; } = new();
    public int TotalSkills { get; private set; }
    public int EcosystemCount { get; private set; }
    public int SourceCount { get; private set; }

    public SkillsModel(ISkillsService skillsService)
    {
        _skillsService = skillsService;
    }

    public void OnGet()
    {
        PageData = _skillsService.GetSkillsPageData();
        TotalSkills = PageData.Categories.Sum(category => category.Skills.Count);
        EcosystemCount = PageData.Categories.Count;
        SourceCount = PageData.RefreshWorkflow.Sources.Count;
    }
}

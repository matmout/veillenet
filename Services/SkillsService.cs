using VeilleNet.Data.SeedData;
using VeilleNet.Models;

namespace VeilleNet.Services;

public interface ISkillsService
{
    SkillPageData GetSkillsPageData();
}

public class SkillsService : ISkillsService
{
    private static readonly Lazy<SkillPageData> _pageData = new(
        () => SeedDataLoader.Load<SkillPageData>("skills.json"));

    public SkillPageData GetSkillsPageData() => _pageData.Value;
}

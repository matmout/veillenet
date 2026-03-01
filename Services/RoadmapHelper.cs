using VeilleNet.Data.SeedData;
using VeilleNet.Models;

namespace VeilleNet.Services;

public static class RoadmapHelper
{
    private static readonly Lazy<List<RoadmapItem>> _learningPath = new(
        () => SeedDataLoader.Load<List<RoadmapItem>>("roadmap.json"));

    public static List<RoadmapItem> GetCSharpLearningPath() => _learningPath.Value;
}

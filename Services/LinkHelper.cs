using VeilleNet.Data.SeedData;
using VeilleNet.Models;

namespace VeilleNet.Services;

public static class LinkHelper
{
    private static readonly Lazy<List<LinkCategory>> _categories = new(
        () => SeedDataLoader.Load<List<LinkCategory>>("links.json"));

    public static List<LinkCategory> GetLinkCategories() => _categories.Value;
}

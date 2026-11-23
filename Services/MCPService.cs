using VeilleNet.Data.SeedData;
using VeilleNet.Models;

namespace VeilleNet.Services;

public interface IMCPService
{
    List<MCPCategory> GetMCPCategories();
}

public class MCPService : IMCPService
{
    private readonly List<MCPCategory> _categories;

    public MCPService()
    {
        _categories = SeedDataLoader.Load<List<MCPCategory>>("mcp-tools.json");
    }

    public List<MCPCategory> GetMCPCategories() => _categories;

}

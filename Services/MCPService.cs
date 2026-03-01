using VeilleNet.Data.SeedData;
using VeilleNet.Models;

namespace VeilleNet.Services;

public interface IMCPService
{
    List<MCPCategory> GetMCPCategories();
    List<MCPTool> GetAllTools();
    List<MCPTool> GetToolsByCategory(string category);
}

public class MCPService : IMCPService
{
    private readonly List<MCPCategory> _categories;

    public MCPService()
    {
        _categories = SeedDataLoader.Load<List<MCPCategory>>("mcp-tools.json");
    }

    public List<MCPCategory> GetMCPCategories() => _categories;

    public List<MCPTool> GetAllTools() =>
        _categories.SelectMany(c => c.Tools).ToList();

    public List<MCPTool> GetToolsByCategory(string category) =>
        _categories
            .FirstOrDefault(c => c.Name.Equals(category, StringComparison.OrdinalIgnoreCase))?
            .Tools ?? new List<MCPTool>();
}

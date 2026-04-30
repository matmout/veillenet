using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;

namespace VeilleNet.Pages;

public class MCPModel : PageModel
{
    private readonly IMCPService _mcpService;

    public List<MCPCategory> Categories { get; set; } = new();
    public string ReviewLabel { get; private set; } = "Reviewed for April 2026";
    public int TotalTools { get; private set; }
    public int TrustedTools { get; private set; }
    public int RemoteReadyTools { get; private set; }

    public MCPModel(IMCPService mcpService)
    {
        _mcpService = mcpService;
    }

    public void OnGet()
    {
        Categories = _mcpService.GetMCPCategories();
        TotalTools = Categories.Sum(category => category.Tools.Count);
        TrustedTools = Categories.Sum(category => category.Tools.Count(tool =>
            tool.TrustLevel.Equals("Official", StringComparison.OrdinalIgnoreCase) ||
            tool.TrustLevel.Equals("Reference", StringComparison.OrdinalIgnoreCase)));
        RemoteReadyTools = Categories.Sum(category => category.Tools.Count(tool =>
            tool.AccessMode.Equals("Remote", StringComparison.OrdinalIgnoreCase) ||
            tool.AccessMode.Equals("Hybrid", StringComparison.OrdinalIgnoreCase)));
    }
}

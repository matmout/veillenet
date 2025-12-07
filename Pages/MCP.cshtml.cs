using Microsoft.AspNetCore.Mvc.RazorPages;
using VeilleNet.Models;
using VeilleNet.Services;

namespace VeilleNet.Pages;

public class MCPModel : PageModel
{
    private readonly IMCPService _mcpService;

    public List<MCPCategory> Categories { get; set; } = new();

    public MCPModel(IMCPService mcpService)
    {
        _mcpService = mcpService;
    }

    public void OnGet()
    {
        Categories = _mcpService.GetMCPCategories();
    }
}

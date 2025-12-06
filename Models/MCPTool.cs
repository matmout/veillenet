namespace VeilleNet.Models;

public class MCPTool
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string GuideUrl { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class MCPCategory
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ColorClass { get; set; } = string.Empty;
    public List<MCPTool> Tools { get; set; } = new();
}

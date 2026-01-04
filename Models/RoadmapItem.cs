namespace VeilleNet.Models;

public class RoadmapItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public RoadmapItemType Type { get; set; }
    public int Step { get; set; } // Numéro d'étape (1, 2, 3, etc.)
    public List<RoadmapItem> Children { get; set; } = new();
    public string? Link { get; set; }
    public List<int> Prerequisites { get; set; } = new(); // Étapes prérequises
}

public enum RoadmapItemType
{
    Foundation,
    Advanced,
    Specialization
}

namespace VeilleNet.Models.Entities;

/// <summary>
/// Marker interface for entities that track creation and modification timestamps.
/// Automatically handled by ApplicationDbContext.SaveChangesAsync.
/// </summary>
public interface IHasTimestamps
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}

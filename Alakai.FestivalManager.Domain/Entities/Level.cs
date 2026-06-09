namespace Alakai.FestivalManager.Domain.Entities;

public class Level : BaseEntity
{
    public Guid PassTypeId { get; set; }

    public PassType PassType { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Pricing

    public decimal EarlyBirdRate { get; set; }

    public decimal GroupRate { get; set; }

    public decimal RegularRate { get; set; }

    // Capacities
    public int? LeaderCapacity { get; set; }

    public int? FollowerCapacity { get; set; }

    public int? IndividualCapacity { get; set; }

    // Balance rules
    public int? MaxLeaderDifference { get; set; }

    public int? MaxFollowerDifference { get; set; }

    // UI
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
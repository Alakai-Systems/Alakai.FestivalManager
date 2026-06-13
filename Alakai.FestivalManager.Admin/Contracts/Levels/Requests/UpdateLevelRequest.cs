using System;

namespace Alakai.FestivalManager.Admin.Contracts.Levels.Requests;

public class UpdateLevelRequest
{
    public Guid PassTypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal EarlyBirdPrice { get; set; }

    public decimal GroupPrice { get; set; }

    public decimal RegularPrice { get; set; }

    public int? LeaderCapacity { get; set; }

    public int? FollowerCapacity { get; set; }

    public int? IndividualCapacity { get; set; }

    public int? MaxLeaderDifference { get; set; }

    public int? MaxFollowerDifference { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}

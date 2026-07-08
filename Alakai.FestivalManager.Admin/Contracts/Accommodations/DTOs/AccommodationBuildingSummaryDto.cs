namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.DTOs;

public class AccommodationBuildingSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; }
    public bool IsLocked { get; set; }
    public bool IsActive { get; set; }
    public int TotalCapacity { get; set; }
    public int ZoneCount { get; set; }
    public int UnitCount { get; set; }
}

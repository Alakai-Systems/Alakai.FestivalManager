namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.DTOs;

public class AccommodationBuildingDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
    public List<string> AllowedPassTypeNames { get; set; } = [];
    public List<AccommodationZoneDto> Zones { get; set; } = [];
}

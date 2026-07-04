namespace Alakai.FestivalManager.Domain.Entities;

public class AccommodationBuilding : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public AccommodationType Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<AccommodationZone> Zones { get; set; } = new List<AccommodationZone>();
    public ICollection<AccommodationBuildingPassType> AllowedPassTypes { get; set; } = new List<AccommodationBuildingPassType>();
}
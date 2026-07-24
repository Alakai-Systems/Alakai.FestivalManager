namespace Alakai.FestivalManager.Domain.Entities;

public class ProductionAccommodationBuilding : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public AccommodationType Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProductionAccommodationZone> Zones { get; set; } = new List<ProductionAccommodationZone>();
}
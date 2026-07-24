namespace Alakai.FestivalManager.Domain.Entities;

public class ProductionAccommodation : BaseEntity
{
    public Guid ProductionAccommodationZoneId { get; set; }
    public ProductionAccommodationZone ProductionAccommodationZone { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
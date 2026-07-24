namespace Alakai.FestivalManager.Domain.Entities;

public class ProductionAccommodationZone : BaseEntity
{
    public Guid ProductionAccommodationBuildingId { get; set; }
    public ProductionAccommodationBuilding ProductionAccommodationBuilding { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ICollection<ProductionAccommodation> Accommodations { get; set; } = new List<ProductionAccommodation>();
}
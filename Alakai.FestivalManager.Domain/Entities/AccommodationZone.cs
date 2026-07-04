namespace Alakai.FestivalManager.Domain.Entities;

public class AccommodationZone : BaseEntity
{
    public Guid AccommodationBuildingId { get; set; }
    public AccommodationBuilding AccommodationBuilding { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ICollection<Accommodation> Accommodations { get; set; } = new List<Accommodation>();
}
namespace Alakai.FestivalManager.Domain.Entities;

public class Accommodation : BaseEntity
{
    public Guid AccommodationZoneId { get; set; }
    public AccommodationZone AccommodationZone { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
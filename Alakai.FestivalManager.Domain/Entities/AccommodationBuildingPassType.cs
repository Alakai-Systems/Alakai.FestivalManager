namespace Alakai.FestivalManager.Domain.Entities;

public class AccommodationBuildingPassType : BaseEntity
{
    public Guid AccommodationBuildingId { get; set; }
    public AccommodationBuilding AccommodationBuilding { get; set; } = default!;

    public Guid PassTypeId { get; set; }
    public PassType PassType { get; set; } = default!;
}
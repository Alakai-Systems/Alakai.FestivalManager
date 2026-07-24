namespace Alakai.FestivalManager.Domain.Entities;

public class ProductionAccommodationReservation : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public Guid ProductionAccommodationBuildingId { get; set; }
    public ProductionAccommodationBuilding ProductionAccommodationBuilding { get; set; } = default!;

    public Guid? ResponsibleProductionPersonId { get; set; }
    public ProductionPerson? ResponsibleProductionPerson { get; set; }

    public int? RoomType { get; set; }

    public ICollection<ProductionAccommodationReservationOccupant> Occupants { get; set; } = new List<ProductionAccommodationReservationOccupant>();
}
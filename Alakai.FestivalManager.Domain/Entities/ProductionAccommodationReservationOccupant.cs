namespace Alakai.FestivalManager.Domain.Entities;

public class ProductionAccommodationReservationOccupant : BaseEntity
{
    public Guid ProductionAccommodationReservationId { get; set; }
    public ProductionAccommodationReservation ProductionAccommodationReservation { get; set; } = default!;

    public Guid ProductionPersonId { get; set; }
    public ProductionPerson ProductionPerson { get; set; } = default!;

    public bool IsResponsible { get; set; }

    public Guid? ProductionAccommodationId { get; set; }
    public ProductionAccommodation? ProductionAccommodation { get; set; }
}
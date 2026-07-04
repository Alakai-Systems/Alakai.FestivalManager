namespace Alakai.FestivalManager.Domain.Entities;

public class AccommodationReservation : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public Guid AccommodationBuildingId { get; set; }
    public AccommodationBuilding AccommodationBuilding { get; set; } = default!;

    public Guid ResponsibleRegistrationId { get; set; }
    public Registration ResponsibleRegistration { get; set; } = default!;

    public ICollection<AccommodationReservationOccupant> Occupants { get; set; } = new List<AccommodationReservationOccupant>();
}
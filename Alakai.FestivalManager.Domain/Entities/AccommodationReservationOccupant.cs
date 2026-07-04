namespace Alakai.FestivalManager.Domain.Entities;

public class AccommodationReservationOccupant : BaseEntity
{
    public Guid AccommodationReservationId { get; set; }
    public AccommodationReservation AccommodationReservation { get; set; } = default!;

    public string Email { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public DateTime? DocumentExpiryDate { get; set; }
    public bool IsResponsible { get; set; }

    public Guid? RegistrationId { get; set; }
    public Registration? Registration { get; set; }

    public Guid? AccommodationId { get; set; }
    public Accommodation? Accommodation { get; set; }
}
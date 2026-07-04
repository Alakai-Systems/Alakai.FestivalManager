namespace Alakai.FestivalManager.Application.Features.Accommodations.Commands;

public class AccommodationOccupantInput
{
    public string Email { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public DateTime? DocumentExpiryDate { get; set; }
}

public class CreateAccommodationReservationCommand
{
    public Guid AccommodationBuildingId { get; set; }
    public Guid ResponsibleRegistrationId { get; set; }
    public List<AccommodationOccupantInput> Occupants { get; set; } = [];
}

public class UpdateAccommodationReservationCommand
{
    public Guid ReservationId { get; set; }
    public Guid RequestingRegistrationId { get; set; }
    public List<AccommodationOccupantInput> Occupants { get; set; } = [];
}
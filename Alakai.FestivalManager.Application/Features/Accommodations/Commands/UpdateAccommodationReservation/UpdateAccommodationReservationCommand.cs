namespace Alakai.FestivalManager.Application.Features.Accommodations.Commands.UpdateAccommodationReservation;

public class UpdateAccommodationReservationCommand
{
    public Guid ReservationId { get; set; }
    public Guid RequestingRegistrationId { get; set; }
    public List<AccommodationOccupantInput> Occupants { get; set; } = [];
}

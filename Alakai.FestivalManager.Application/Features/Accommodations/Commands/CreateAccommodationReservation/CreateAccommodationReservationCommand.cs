namespace Alakai.FestivalManager.Application.Features.Accommodations.Commands.CreateAccommodationReservation;

public class CreateAccommodationReservationCommand
{
    public Guid AccommodationBuildingId { get; set; }
    public Guid ResponsibleRegistrationId { get; set; }
    public List<AccommodationOccupantInput> Occupants { get; set; } = [];
}

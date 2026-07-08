namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Requests;

public class UpdateAccommodationReservationRequest
{
    public Guid RequestingRegistrationId { get; set; }
    public List<AccommodationOccupantInput> Occupants { get; set; } = [];
}

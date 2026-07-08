namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Requests;

public class CreateAccommodationReservationRequest
{
    public Guid AccommodationBuildingId { get; set; }
    public Guid ResponsibleRegistrationId { get; set; }
    public List<AccommodationOccupantInput> Occupants { get; set; } = [];
}

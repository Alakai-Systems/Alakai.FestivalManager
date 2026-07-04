namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Requests;

public class AccommodationOccupantInput
{
    public string Email { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public DateTime? DocumentExpiryDate { get; set; }
}

public class CreateAccommodationReservationRequest
{
    public Guid AccommodationBuildingId { get; set; }
    public Guid ResponsibleRegistrationId { get; set; }
    public List<AccommodationOccupantInput> Occupants { get; set; } = [];
}

public class UpdateAccommodationReservationRequest
{
    public Guid RequestingRegistrationId { get; set; }
    public List<AccommodationOccupantInput> Occupants { get; set; } = [];
}
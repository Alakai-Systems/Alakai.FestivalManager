namespace Alakai.FestivalManager.Application.Features.Accommodations.Contracts.DTOs;

public class AccommodationReservationDto
{
    public Guid Id { get; set; }
    public Guid AccommodationBuildingId { get; set; }
    public string? BuildingName { get; set; }
    public Guid ResponsibleRegistrationId { get; set; }
    public string? ResponsibleName { get; set; }
    public List<AccommodationReservationOccupantDto> Occupants { get; set; } = [];
}

namespace Alakai.FestivalManager.Application.Features.Accommodations.Contracts.DTOs;

public class AccommodationReservationOccupantDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public DateTime? DocumentExpiryDate { get; set; }
    public bool IsResponsible { get; set; }
    public Guid? RegistrationId { get; set; }
    public string? RegistrationName { get; set; }
    public Guid? AccommodationId { get; set; }
    public string? AccommodationName { get; set; }
    public string? ZoneName { get; set; }
}

public class AccommodationReservationDto
{
    public Guid Id { get; set; }
    public Guid AccommodationBuildingId { get; set; }
    public string? BuildingName { get; set; }
    public Guid ResponsibleRegistrationId { get; set; }
    public string? ResponsibleName { get; set; }
    public List<AccommodationReservationOccupantDto> Occupants { get; set; } = [];
}
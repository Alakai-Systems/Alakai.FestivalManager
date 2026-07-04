namespace Alakai.FestivalManager.Application.Features.Buses.Contracts.DTOs;

public class BusReservationDto
{
    public Guid Id { get; set; }
    public Guid BusId { get; set; }
    public BusDirection Direction { get; set; }
    public DateTime DepartureTime { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DestinationLocation { get; set; } = string.Empty;
    public Guid RegistrationId { get; set; }
    public string? RegistrationName { get; set; }
    public string? RegistrationEmail { get; set; }
}
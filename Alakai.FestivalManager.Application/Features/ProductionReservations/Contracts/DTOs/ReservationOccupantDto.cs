namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Contracts.DTOs;

public class ReservationOccupantDto
{
    public Guid Id { get; set; }
    public Guid ProductionPersonId { get; set; }
    public string? ProductionPersonName { get; set; }
    public bool IsResponsible { get; set; }
    public Guid? ProductionAccommodationId { get; set; }
    public string? AccommodationName { get; set; }
    public string? ZoneName { get; set; }
}
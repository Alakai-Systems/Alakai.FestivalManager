namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Contracts.Responses;

public class GetReservationsResponse
{
    public IReadOnlyList<ReservationDto> Reservations { get; set; } = [];
}
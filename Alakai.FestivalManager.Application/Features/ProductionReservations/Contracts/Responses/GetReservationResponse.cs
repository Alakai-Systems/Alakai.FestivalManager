namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Contracts.Responses;

public class GetReservationResponse
{
    public ReservationDto Reservation { get; set; } = default!;
}
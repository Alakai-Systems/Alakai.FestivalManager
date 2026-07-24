namespace Alakai.FestivalManager.Admin.Contracts.ProductionReservations.Responses;

public class GetReservationResponse
{
    public ReservationDto Reservation { get; set; } = new ReservationDto();
}
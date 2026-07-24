namespace Alakai.FestivalManager.Admin.Contracts.ProductionReservations.Responses;

public class GetReservationsResponse
{
    public IReadOnlyList<ReservationDto> Reservations { get; set; } = new List<ReservationDto>();
}
namespace Alakai.FestivalManager.Admin.Contracts.Buses.Responses;

public class GetBusReservationsResponse
{
    public List<BusReservationDto> Reservations { get; set; } = [];
}

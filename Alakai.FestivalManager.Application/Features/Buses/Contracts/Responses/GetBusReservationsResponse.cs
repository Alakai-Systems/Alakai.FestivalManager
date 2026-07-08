namespace Alakai.FestivalManager.Application.Features.Buses.Contracts.Responses;

public class GetBusReservationsResponse
{
    public List<BusReservationDto> Reservations { get; set; } = [];
}

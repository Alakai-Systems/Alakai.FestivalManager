using Alakai.FestivalManager.Application.Features.Buses.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Buses.Contracts.Responses;

public class CreateBusReservationResponse
{
    public BusReservationDto Reservation { get; set; } = default!;
}

public class GetBusReservationsResponse
{
    public List<BusReservationDto> Reservations { get; set; } = [];
}

public class DeleteBusReservationResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
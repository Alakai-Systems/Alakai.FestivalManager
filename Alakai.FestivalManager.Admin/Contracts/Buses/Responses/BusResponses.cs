using Alakai.FestivalManager.Admin.Contracts.Buses.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Buses.Responses;

public class GetBusesResponse
{
    public List<BusDto> Buses { get; set; } = [];
}

public class GetBusResponse
{
    public BusDto Bus { get; set; } = default!;
}

public class CreateBusResponse
{
    public BusDto Bus { get; set; } = default!;
}

public class UpdateBusResponse
{
    public BusDto Bus { get; set; } = default!;
}

public class DeleteBusResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}

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
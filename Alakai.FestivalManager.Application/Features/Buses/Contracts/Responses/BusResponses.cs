using Alakai.FestivalManager.Application.Features.Buses.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Buses.Contracts.Responses;

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
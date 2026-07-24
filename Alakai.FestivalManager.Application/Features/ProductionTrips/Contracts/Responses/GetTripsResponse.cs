namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Contracts.Responses;

public class GetTripsResponse
{
    public IReadOnlyList<ProductionTripDto> Trips { get; set; } = [];
}
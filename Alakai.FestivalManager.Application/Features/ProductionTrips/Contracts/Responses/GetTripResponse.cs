namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Contracts.Responses;

public class GetTripResponse
{
    public ProductionTripDto Trip { get; set; } = default!;
}
namespace Alakai.FestivalManager.Admin.Contracts.ProductionTrips.Responses;

public class GetTripsResponse
{
    public IReadOnlyList<ProductionTripDto> Trips { get; set; } = new List<ProductionTripDto>();
}
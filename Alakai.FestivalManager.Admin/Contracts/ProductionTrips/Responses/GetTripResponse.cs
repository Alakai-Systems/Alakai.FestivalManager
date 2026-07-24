namespace Alakai.FestivalManager.Admin.Contracts.ProductionTrips.Responses;

public class GetTripResponse
{
    public ProductionTripDto Trip { get; set; } = new ProductionTripDto();
}
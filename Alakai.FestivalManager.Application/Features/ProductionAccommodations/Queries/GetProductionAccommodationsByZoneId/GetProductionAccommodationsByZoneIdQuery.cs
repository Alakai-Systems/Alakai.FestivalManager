namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Queries.GetProductionAccommodationsByZoneId;

public class GetProductionAccommodationsByZoneIdQuery
{
    public Guid ProductionAccommodationZoneId { get; set; }
    public GetProductionAccommodationsByZoneIdQuery(Guid productionAccommodationZoneId)
    {
        ProductionAccommodationZoneId = productionAccommodationZoneId;
    }
}
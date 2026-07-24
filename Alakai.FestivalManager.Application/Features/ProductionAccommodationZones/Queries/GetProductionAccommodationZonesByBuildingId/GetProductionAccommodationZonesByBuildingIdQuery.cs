namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Queries.GetProductionAccommodationZonesByBuildingId;

public class GetProductionAccommodationZonesByBuildingIdQuery
{
    public Guid ProductionAccommodationBuildingId { get; set; }
    public GetProductionAccommodationZonesByBuildingIdQuery(Guid productionAccommodationBuildingId)
    {
        ProductionAccommodationBuildingId = productionAccommodationBuildingId;
    }
}
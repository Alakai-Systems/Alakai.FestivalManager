namespace Alakai.FestivalManager.Admin.Contracts.ProductionZones.Responses;

public class GetProductionAccommodationZonesResponse
{
    public IReadOnlyList<ProductionAccommodationZoneDto> ProductionAccommodationZones { get; set; } = new List<ProductionAccommodationZoneDto>();
}
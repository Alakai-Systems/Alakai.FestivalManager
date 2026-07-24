namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Contracts.Responses;

public class GetProductionAccommodationZonesResponse
{
    public IReadOnlyList<ProductionAccommodationZoneDto> ProductionAccommodationZones { get; set; } = [];
}
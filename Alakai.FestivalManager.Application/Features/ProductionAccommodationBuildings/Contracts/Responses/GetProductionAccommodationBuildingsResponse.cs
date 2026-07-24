namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Contracts.Responses;

public class GetProductionAccommodationBuildingsResponse
{
    public IReadOnlyList<ProductionAccommodationBuildingDto> ProductionAccommodationBuildings { get; set; } = [];
}
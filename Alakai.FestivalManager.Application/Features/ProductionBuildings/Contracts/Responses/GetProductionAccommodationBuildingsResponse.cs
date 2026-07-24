namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Contracts.Responses;

public class GetProductionAccommodationBuildingsResponse
{
    public IReadOnlyList<ProductionAccommodationBuildingDto> ProductionAccommodationBuildings { get; set; } = [];
}
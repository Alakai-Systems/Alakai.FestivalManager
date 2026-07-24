namespace Alakai.FestivalManager.Admin.Contracts.ProductionBuildings.Responses;

public class GetProductionAccommodationBuildingsResponse
{
    public IReadOnlyList<ProductionAccommodationBuildingDto> ProductionAccommodationBuildings { get; set; } = new List<ProductionAccommodationBuildingDto>();
}
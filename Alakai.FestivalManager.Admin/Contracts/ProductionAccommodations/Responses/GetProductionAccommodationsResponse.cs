namespace Alakai.FestivalManager.Admin.Contracts.ProductionAccommodations.Responses;

public class GetProductionAccommodationsResponse
{
    public IReadOnlyList<ProductionAccommodationDto> ProductionAccommodations { get; set; } = new List<ProductionAccommodationDto>();
}
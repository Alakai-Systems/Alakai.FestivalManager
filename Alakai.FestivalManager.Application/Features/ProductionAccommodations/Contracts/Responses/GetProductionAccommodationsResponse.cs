namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Contracts.Responses;

public class GetProductionAccommodationsResponse
{
    public IReadOnlyList<ProductionAccommodationDto> ProductionAccommodations { get; set; } = [];
}
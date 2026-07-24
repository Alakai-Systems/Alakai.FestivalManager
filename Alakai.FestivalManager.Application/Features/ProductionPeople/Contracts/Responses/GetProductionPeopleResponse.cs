namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Contracts.Responses;

public class GetProductionPeopleResponse
{
    public IReadOnlyList<ProductionPersonDto> ProductionPeople { get; set; } = [];
}
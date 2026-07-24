namespace Alakai.FestivalManager.Admin.Contracts.ProductionPeople.Responses;

public class GetProductionPeopleResponse
{
    public IReadOnlyList<ProductionPersonDto> ProductionPeople { get; set; } = new List<ProductionPersonDto>();
}
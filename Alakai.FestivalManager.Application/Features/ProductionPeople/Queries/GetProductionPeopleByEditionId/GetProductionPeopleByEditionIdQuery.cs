namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Queries.GetProductionPeopleByEditionId;

public class GetProductionPeopleByEditionIdQuery
{
    public Guid EditionId { get; set; }
    public GetProductionPeopleByEditionIdQuery(Guid editionId)
    {
        EditionId = editionId;
    }
}
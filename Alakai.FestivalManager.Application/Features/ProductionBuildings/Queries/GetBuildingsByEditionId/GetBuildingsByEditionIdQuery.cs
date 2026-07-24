namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Queries.GetBuildingsByEditionId;

public class GetBuildingsByEditionIdQuery
{
    public Guid EditionId { get; set; }
    public GetBuildingsByEditionIdQuery(Guid editionId)
    {
        EditionId = editionId;
    }
}
namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Queries.GetProductionAccommodationBuildingsByEditionId;

public class GetProductionAccommodationBuildingsByEditionIdQuery
{
    public Guid EditionId { get; set; }
    public GetProductionAccommodationBuildingsByEditionIdQuery(Guid editionId)
    {
        EditionId = editionId;
    }
}
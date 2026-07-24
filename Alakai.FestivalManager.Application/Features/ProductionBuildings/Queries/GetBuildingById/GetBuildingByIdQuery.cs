namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Queries.GetBuildingById;

public class GetBuildingByIdQuery
{
    public Guid Id { get; set; }
    public GetBuildingByIdQuery(Guid id)
    {
        Id = id;
    }
}
namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Queries.GetProductionAccommodationBuildingById;

public class GetProductionAccommodationBuildingByIdQuery
{
    public Guid Id { get; set; }
    public GetProductionAccommodationBuildingByIdQuery(Guid id)
    {
        Id = id;
    }
}
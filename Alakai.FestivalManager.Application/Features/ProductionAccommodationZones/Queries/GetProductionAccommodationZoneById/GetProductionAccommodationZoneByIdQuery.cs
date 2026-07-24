namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Queries.GetProductionAccommodationZoneById;

public class GetProductionAccommodationZoneByIdQuery
{
    public Guid Id { get; set; }
    public GetProductionAccommodationZoneByIdQuery(Guid id)
    {
        Id = id;
    }
}
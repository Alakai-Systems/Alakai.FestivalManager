namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Queries.GetProductionAccommodationById;

public class GetProductionAccommodationByIdQuery
{
    public Guid Id { get; set; }
    public GetProductionAccommodationByIdQuery(Guid id)
    {
        Id = id;
    }
}
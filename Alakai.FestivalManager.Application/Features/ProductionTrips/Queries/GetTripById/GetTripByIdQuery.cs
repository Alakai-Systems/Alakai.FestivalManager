namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Queries.GetTripById;

public class GetTripByIdQuery
{
    public Guid Id { get; set; }
    public GetTripByIdQuery(Guid id)
    {
        Id = id;
    }
}
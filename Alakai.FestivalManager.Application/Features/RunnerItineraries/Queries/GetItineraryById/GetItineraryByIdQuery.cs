namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Queries.GetItineraryById;

public class GetItineraryByIdQuery
{
    public Guid Id { get; set; }
    public GetItineraryByIdQuery(Guid id)
    {
        Id = id;
    }
}
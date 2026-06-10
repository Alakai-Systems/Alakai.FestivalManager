namespace Alakai.FestivalManager.Application.Features.Festivals.Queries.GetFestivalById;

public class GetFestivalByIdQuery
{
    public Guid Id { get; set; }

    public GetFestivalByIdQuery(Guid id)
    {
        Id = id;
    }
}

namespace Alakai.FestivalManager.Application.Features.Levels.Queries.GetLevelById;

public class GetLevelByIdQuery
{
    public Guid Id { get; set; }
    public GetLevelByIdQuery(Guid id)
    {
        Id = id;
    }
}

namespace Alakai.FestivalManager.Application.Features.Editions.Queries.GetEditionById;

public class GetEditionByIdQuery
{
    public Guid Id { get; set; }
    public GetEditionByIdQuery(Guid id)
    {
        Id = id;
    }
}

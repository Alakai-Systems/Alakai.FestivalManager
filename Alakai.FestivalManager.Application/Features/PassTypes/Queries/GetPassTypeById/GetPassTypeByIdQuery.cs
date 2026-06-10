namespace Alakai.FestivalManager.Application.Features.PassTypes.Queries.GetPassTypeById;

public class GetPassTypeByIdQuery
{
    public Guid Id { get; set; }
    public GetPassTypeByIdQuery(Guid id)
    {
        Id = id;
    }
}
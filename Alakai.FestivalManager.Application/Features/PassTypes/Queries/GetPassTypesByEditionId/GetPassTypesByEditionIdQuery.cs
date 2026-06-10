namespace Alakai.FestivalManager.Application.Features.PassTypes.Queries.GetPassTypesByEditionId;

public class GetPassTypesByEditionIdQuery
{
    public Guid EditionId { get; set; }
    public GetPassTypesByEditionIdQuery(Guid editionId)
    {
        EditionId = editionId;
    }
}

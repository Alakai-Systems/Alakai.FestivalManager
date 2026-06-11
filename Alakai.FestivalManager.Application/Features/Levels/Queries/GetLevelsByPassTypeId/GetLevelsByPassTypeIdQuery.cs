namespace Alakai.FestivalManager.Application.Features.Levels.Queries.GetLevelsByPassTypeId;

public class GetLevelsByPassTypeIdQuery
{
    public Guid PassTypeId { get; set; }
    public GetLevelsByPassTypeIdQuery(Guid passTypeId)
    {
        PassTypeId = passTypeId;
    }
}

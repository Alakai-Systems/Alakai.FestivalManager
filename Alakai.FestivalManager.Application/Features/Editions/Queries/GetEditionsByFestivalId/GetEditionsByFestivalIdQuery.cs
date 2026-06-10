namespace Alakai.FestivalManager.Application.Features.Editions.Queries.GetEditionsByFestivalId;
public class GetEditionsByFestivalIdQuery
{
    public Guid FestivalId { get; set; }
    public GetEditionsByFestivalIdQuery(Guid festivalId)
    {
        FestivalId = festivalId;
    }
}
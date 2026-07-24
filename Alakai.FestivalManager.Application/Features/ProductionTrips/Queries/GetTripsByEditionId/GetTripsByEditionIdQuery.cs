namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Queries.GetTripsByEditionId;

public class GetTripsByEditionIdQuery
{
    public Guid EditionId { get; set; }
    public GetTripsByEditionIdQuery(Guid editionId)
    {
        EditionId = editionId;
    }
}
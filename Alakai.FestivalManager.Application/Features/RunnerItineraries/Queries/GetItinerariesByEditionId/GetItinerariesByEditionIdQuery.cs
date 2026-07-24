namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Queries.GetItinerariesByEditionId;

public class GetItinerariesByEditionIdQuery
{
    public Guid EditionId { get; set; }
    public GetItinerariesByEditionIdQuery(Guid editionId)
    {
        EditionId = editionId;
    }
}
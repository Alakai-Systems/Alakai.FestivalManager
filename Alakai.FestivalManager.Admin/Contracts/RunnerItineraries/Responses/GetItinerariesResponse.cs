namespace Alakai.FestivalManager.Admin.Contracts.RunnerItineraries.Responses;

public class GetItinerariesResponse
{
    public IReadOnlyList<RunnerItineraryDto> Itineraries { get; set; } = new List<RunnerItineraryDto>();
}
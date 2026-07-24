namespace Alakai.FestivalManager.Admin.Contracts.RunnerItineraries.Responses;

public class GetItineraryResponse
{
    public RunnerItineraryDto Itinerary { get; set; } = new RunnerItineraryDto();
}
namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Contracts.Responses;

public class GetItineraryResponse
{
    public RunnerItineraryDto Itinerary { get; set; } = default!;
}
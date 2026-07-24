namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Contracts.Responses;

public class GetItinerariesResponse
{
    public IReadOnlyList<RunnerItineraryDto> Itineraries { get; set; } = [];
}
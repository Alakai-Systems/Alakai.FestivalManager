namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Contracts.Responses;

public class DeleteItineraryResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
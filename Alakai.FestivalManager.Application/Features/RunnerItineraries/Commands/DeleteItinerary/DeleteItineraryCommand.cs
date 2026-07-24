namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Commands.DeleteItinerary;

public class DeleteItineraryCommand
{
    public Guid Id { get; set; }
    public DeleteItineraryCommand(Guid id)
    {
        Id = id;
    }
}
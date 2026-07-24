namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Commands.UpdateItinerary;

public class UpdateItineraryCommand
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public TripDirection Direction { get; set; }
    public string? RunnerName { get; set; }
    public string? Notes { get; set; }
}
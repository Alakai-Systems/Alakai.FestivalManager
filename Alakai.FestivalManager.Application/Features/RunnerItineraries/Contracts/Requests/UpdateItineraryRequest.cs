namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Contracts.Requests;

public class UpdateItineraryRequest
{
    public DateTime DateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public TripDirection Direction { get; set; }
    public string? RunnerName { get; set; }
    public string? Notes { get; set; }
}
namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Contracts.DTOs;

public class RunnerItineraryDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public DateTime DateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public TripDirection Direction { get; set; }
    public string? RunnerName { get; set; }
    public string? Notes { get; set; }
    public List<ProductionTripDto> Trips { get; set; } = [];
}
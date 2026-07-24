namespace Alakai.FestivalManager.Admin.Contracts.RunnerItineraries.DTOs;

public class RunnerItineraryDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public DateTime DateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Direction { get; set; }
    public string? RunnerName { get; set; }
    public string? Notes { get; set; }
    public List<ProductionTripDto> Trips { get; set; } = new List<ProductionTripDto>();
}
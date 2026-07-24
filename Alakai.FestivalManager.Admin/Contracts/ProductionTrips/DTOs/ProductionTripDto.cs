namespace Alakai.FestivalManager.Admin.Contracts.ProductionTrips.DTOs;

public class ProductionTripDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public Guid ProductionPersonId { get; set; }
    public string? ProductionPersonName { get; set; }
    public int Type { get; set; }
    public string TripNumber { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string TerminalOrStation { get; set; } = string.Empty;
    public int Direction { get; set; }
    public Guid? RunnerItineraryId { get; set; }
}
namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Contracts.Requests;

public class UpdateTripRequest
{
    public Guid ProductionPersonId { get; set; }
    public ProductionTripType Type { get; set; }
    public string TripNumber { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string TerminalOrStation { get; set; } = string.Empty;
    public TripDirection Direction { get; set; }
    public Guid? RunnerItineraryId { get; set; }
}
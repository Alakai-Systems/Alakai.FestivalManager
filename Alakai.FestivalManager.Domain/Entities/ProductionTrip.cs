namespace Alakai.FestivalManager.Domain.Entities;

public class ProductionTrip : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public Guid ProductionPersonId { get; set; }
    public ProductionPerson ProductionPerson { get; set; } = default!;

    public ProductionTripType Type { get; set; }
    public string TripNumber { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
    public string TerminalOrStation { get; set; } = string.Empty;
    public TripDirection Direction { get; set; }

    public Guid? RunnerItineraryId { get; set; }
    public RunnerItinerary? RunnerItinerary { get; set; }
}
namespace Alakai.FestivalManager.Domain.Entities;

public class RunnerItinerary : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public DateTime DateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public TripDirection Direction { get; set; }
    public string? RunnerName { get; set; }
    public string? Notes { get; set; }

    public ICollection<ProductionTrip> Trips { get; set; } = new List<ProductionTrip>();
}
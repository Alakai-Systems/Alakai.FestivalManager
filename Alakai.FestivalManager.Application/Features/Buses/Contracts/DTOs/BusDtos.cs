namespace Alakai.FestivalManager.Application.Features.Buses.Contracts.DTOs;

public class BusDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public BusDirection Direction { get; set; }
    public DateTime DepartureTime { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DestinationLocation { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int OccupiedCount { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
    public List<string> AllowedPassTypeNames { get; set; } = [];
}
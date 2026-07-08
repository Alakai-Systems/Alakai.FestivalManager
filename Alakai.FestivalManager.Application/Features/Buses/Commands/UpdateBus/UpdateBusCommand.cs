namespace Alakai.FestivalManager.Application.Features.Buses.Commands.UpdateBus;

public class UpdateBusCommand
{
    public Guid Id { get; set; }
    public BusDirection Direction { get; set; }
    public DateTime DepartureTime { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DestinationLocation { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
}

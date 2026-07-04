namespace Alakai.FestivalManager.Domain.Entities;

public class Bus : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public BusDirection Direction { get; set; }
    public DateTime DepartureTime { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DestinationLocation { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<BusPassType> AllowedPassTypes { get; set; } = new List<BusPassType>();
    public ICollection<BusReservation> Reservations { get; set; } = new List<BusReservation>();
}
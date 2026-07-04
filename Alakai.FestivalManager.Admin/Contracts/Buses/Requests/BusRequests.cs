namespace Alakai.FestivalManager.Admin.Contracts.Buses.Requests;

public class CreateBusRequest
{
    public Guid EditionId { get; set; }
    public int Direction { get; set; } = 1;
    public DateTime DepartureTime { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DestinationLocation { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
}

public class UpdateBusRequest
{
    public int Direction { get; set; }
    public DateTime DepartureTime { get; set; }
    public string PickupLocation { get; set; } = string.Empty;
    public string DestinationLocation { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
}

public class CreateBusReservationRequest
{
    public Guid BusId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class CreateBusReservationsRequest
{
    public Guid RegistrationId { get; set; }
    public List<Guid> BusIds { get; set; } = [];
}

public class UpdateBusReservationRequest
{
    public Guid NewBusId { get; set; }
    public Guid RequestingRegistrationId { get; set; }
}
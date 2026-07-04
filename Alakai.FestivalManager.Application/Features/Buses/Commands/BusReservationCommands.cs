namespace Alakai.FestivalManager.Application.Features.Buses.Commands;

public class CreateBusReservationCommand
{
    public Guid BusId { get; set; }
    public Guid RegistrationId { get; set; }
}

public class UpdateBusReservationCommand
{
    public Guid ReservationId { get; set; }
    public Guid NewBusId { get; set; }
    public Guid RequestingRegistrationId { get; set; }
}
public class CreateBusReservationsCommand
{
    public Guid RegistrationId { get; set; }
    public List<Guid> BusIds { get; set; } = [];
}

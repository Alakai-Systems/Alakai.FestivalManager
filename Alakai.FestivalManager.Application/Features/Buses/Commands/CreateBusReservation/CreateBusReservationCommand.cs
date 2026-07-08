namespace Alakai.FestivalManager.Application.Features.Buses.Commands.CreateBusReservation;

public class CreateBusReservationCommand
{
    public Guid BusId { get; set; }
    public Guid RegistrationId { get; set; }
}

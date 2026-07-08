namespace Alakai.FestivalManager.Application.Features.Buses.Commands.CreateBusReservations;

public class CreateBusReservationsCommand
{
    public Guid RegistrationId { get; set; }
    public List<Guid> BusIds { get; set; } = [];
}

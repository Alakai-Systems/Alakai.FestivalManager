namespace Alakai.FestivalManager.Application.Features.Buses.Commands.UpdateBusReservation;

public class UpdateBusReservationCommand
{
    public Guid ReservationId { get; set; }
    public Guid NewBusId { get; set; }
    public Guid RequestingRegistrationId { get; set; }
}

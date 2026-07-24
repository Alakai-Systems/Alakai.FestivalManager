namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Commands.DeleteReservation;

public class DeleteReservationCommand
{
    public Guid Id { get; set; }
    public DeleteReservationCommand(Guid id)
    {
        Id = id;
    }
}
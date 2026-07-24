namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Commands.UpdateReservation;

public class UpdateReservationCommand
{
    public Guid ReservationId { get; set; }
    public Guid? ResponsibleProductionPersonId { get; set; }
    public List<ReservationOccupantInput> Occupants { get; set; } = [];
}
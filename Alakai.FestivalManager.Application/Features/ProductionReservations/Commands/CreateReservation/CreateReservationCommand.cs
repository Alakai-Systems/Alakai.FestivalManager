namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Commands.CreateReservation;

public class CreateReservationCommand
{
    public Guid EditionId { get; set; }
    public Guid ProductionAccommodationBuildingId { get; set; }
    public Guid? ResponsibleProductionPersonId { get; set; }
    public List<ReservationOccupantInput> Occupants { get; set; } = [];
}
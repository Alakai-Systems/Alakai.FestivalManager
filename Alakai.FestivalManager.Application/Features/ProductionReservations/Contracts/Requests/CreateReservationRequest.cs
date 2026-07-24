namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Contracts.Requests;

public class CreateReservationRequest
{
    public Guid EditionId { get; set; }
    public Guid ProductionAccommodationBuildingId { get; set; }
    public Guid? ResponsibleProductionPersonId { get; set; }
    public List<ReservationOccupantInput> Occupants { get; set; } = [];
}
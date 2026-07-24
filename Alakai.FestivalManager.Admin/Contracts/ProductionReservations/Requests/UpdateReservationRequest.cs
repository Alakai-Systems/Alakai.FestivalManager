namespace Alakai.FestivalManager.Admin.Contracts.ProductionReservations.Requests;

public class UpdateReservationRequest
{
    public Guid? ResponsibleProductionPersonId { get; set; }
    public List<ReservationOccupantInput> Occupants { get; set; } = new List<ReservationOccupantInput>();
}
namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Contracts.Requests;

public class UpdateReservationRequest
{
    public Guid? ResponsibleProductionPersonId { get; set; }
    public int? RoomType { get; set; }
    public List<ReservationOccupantInput> Occupants { get; set; } = [];
}
namespace Alakai.FestivalManager.Admin.Contracts.ProductionReservations.Requests;

public class ReservationOccupantInput
{
    public Guid ProductionPersonId { get; set; }
    public Guid? ProductionAccommodationId { get; set; }
}
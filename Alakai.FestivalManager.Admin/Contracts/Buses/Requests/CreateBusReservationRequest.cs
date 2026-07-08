namespace Alakai.FestivalManager.Admin.Contracts.Buses.Requests;

public class CreateBusReservationRequest
{
    public Guid BusId { get; set; }
    public Guid RegistrationId { get; set; }
}

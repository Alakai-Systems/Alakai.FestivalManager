namespace Alakai.FestivalManager.Admin.Contracts.Buses.Requests;

public class CreateBusReservationsRequest
{
    public Guid RegistrationId { get; set; }
    public List<Guid> BusIds { get; set; } = [];
}

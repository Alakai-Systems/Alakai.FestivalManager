namespace Alakai.FestivalManager.Admin.Contracts.Buses.Requests;

public class UpdateBusReservationRequest
{
    public Guid NewBusId { get; set; }
    public Guid RequestingRegistrationId { get; set; }
}

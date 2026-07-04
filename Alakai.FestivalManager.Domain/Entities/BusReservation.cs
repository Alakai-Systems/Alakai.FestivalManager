namespace Alakai.FestivalManager.Domain.Entities;

public class BusReservation : BaseEntity
{
    public Guid BusId { get; set; }
    public Bus Bus { get; set; } = default!;

    public Guid RegistrationId { get; set; }
    public Registration Registration { get; set; } = default!;
}
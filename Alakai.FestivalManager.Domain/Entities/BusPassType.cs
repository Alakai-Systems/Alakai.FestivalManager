namespace Alakai.FestivalManager.Domain.Entities;

public class BusPassType : BaseEntity
{
    public Guid BusId { get; set; }
    public Bus Bus { get; set; } = default!;

    public Guid PassTypeId { get; set; }
    public PassType PassType { get; set; } = default!;
}
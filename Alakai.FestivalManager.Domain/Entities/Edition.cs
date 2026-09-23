namespace Alakai.FestivalManager.Domain.Entities;

public class Edition : BaseEntity
{
    public Guid FestivalId { get; set; }
    public Festival Festival { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RegistrationOpenDate { get; set; }
    public DateTime? RegistrationCloseDate { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<PassType> PassTypes { get; set; } = new List<PassType>();

    // Cupo Early Bird compartido por toda la edicion (todos los pases/niveles).
    // Null o 0 = Early Bird desactivado; se aplica EarlyBirdPrice mientras
    // EarlyBirdUsedCount < EarlyBirdCapacity, luego cae a RegularPrice.
    public int? EarlyBirdCapacity { get; set; }
    public int EarlyBirdUsedCount { get; set; } = 0;

    // URL publica del PDF de horarios de esta edicion (null = sin subir).
    public string? ScheduleUrl { get; set; }
}

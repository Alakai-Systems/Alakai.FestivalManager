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
}

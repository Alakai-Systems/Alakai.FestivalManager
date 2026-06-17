namespace Alakai.FestivalManager.Domain.Entities;

public class DiscountCode : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }

    public DiscountActivationType ActivationType { get; set; }
    public int? ActivationThreshold { get; set; }

    public ICollection<Registration> Registrations { get; set; } = [];

    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }

    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public bool IsActive { get; set; } = true;
}

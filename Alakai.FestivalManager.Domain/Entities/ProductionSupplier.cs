namespace Alakai.FestivalManager.Domain.Entities;

public class ProductionSupplier : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
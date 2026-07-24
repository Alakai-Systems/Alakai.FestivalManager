namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Commands.UpdateProductionSupplier;

public class UpdateProductionSupplierCommand
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}
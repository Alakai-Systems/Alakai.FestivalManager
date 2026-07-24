namespace Alakai.FestivalManager.Admin.Contracts.ProductionSuppliers.Responses;

public class GetProductionSuppliersResponse
{
    public IReadOnlyList<ProductionSupplierDto> ProductionSuppliers { get; set; } = new List<ProductionSupplierDto>();
}
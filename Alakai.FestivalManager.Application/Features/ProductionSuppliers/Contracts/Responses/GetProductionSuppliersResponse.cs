namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Contracts.Responses;

public class GetProductionSuppliersResponse
{
    public IReadOnlyList<ProductionSupplierDto> ProductionSuppliers { get; set; } = [];
}
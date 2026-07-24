namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Queries.GetProductionSuppliersByEditionId;

public class GetProductionSuppliersByEditionIdQuery
{
    public Guid EditionId { get; set; }
    public GetProductionSuppliersByEditionIdQuery(Guid editionId)
    {
        EditionId = editionId;
    }
}
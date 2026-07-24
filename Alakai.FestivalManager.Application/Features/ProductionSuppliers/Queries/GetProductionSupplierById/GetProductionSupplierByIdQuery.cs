namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Queries.GetProductionSupplierById;

public class GetProductionSupplierByIdQuery
{
    public Guid Id { get; set; }
    public GetProductionSupplierByIdQuery(Guid id)
    {
        Id = id;
    }
}
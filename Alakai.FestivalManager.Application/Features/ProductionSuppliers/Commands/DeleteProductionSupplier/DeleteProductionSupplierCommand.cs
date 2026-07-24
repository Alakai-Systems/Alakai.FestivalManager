namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Commands.DeleteProductionSupplier;

public class DeleteProductionSupplierCommand
{
    public Guid Id { get; set; }
    public DeleteProductionSupplierCommand(Guid id)
    {
        Id = id;
    }
}
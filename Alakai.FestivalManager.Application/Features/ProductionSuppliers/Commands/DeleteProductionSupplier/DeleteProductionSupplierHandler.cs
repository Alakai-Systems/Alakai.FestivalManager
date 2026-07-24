namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Commands.DeleteProductionSupplier;

public class DeleteProductionSupplierHandler
{
    private readonly IProductionSupplierRepository _productionSupplierRepository;

    public DeleteProductionSupplierHandler(IProductionSupplierRepository productionSupplierRepository)
    {
        _productionSupplierRepository = productionSupplierRepository;
    }

    public async Task<Guid> HandleAsync(DeleteProductionSupplierCommand command, CancellationToken cancellationToken = default)
    {
        ProductionSupplier? productionSupplier = await _productionSupplierRepository.GetByIdAsync(command.Id, cancellationToken);

        if (productionSupplier is null)
        {
            throw new NotFoundException($"Production supplier with id '{command.Id}' was not found.");
        }

        _productionSupplierRepository.Delete(productionSupplier);

        await _productionSupplierRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}
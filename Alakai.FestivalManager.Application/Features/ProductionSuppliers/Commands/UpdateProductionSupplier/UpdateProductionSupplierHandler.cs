namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Commands.UpdateProductionSupplier;

public class UpdateProductionSupplierHandler
{
    private readonly IProductionSupplierRepository _productionSupplierRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdateProductionSupplierHandler(IProductionSupplierRepository productionSupplierRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _productionSupplierRepository = productionSupplierRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<ProductionSupplierDto> HandleAsync(UpdateProductionSupplierCommand command, CancellationToken cancellationToken = default)
    {
        ProductionSupplier? productionSupplier = await _productionSupplierRepository.GetByIdAsync(command.Id, cancellationToken);

        if (productionSupplier is null)
        {
            throw new NotFoundException($"Production supplier with id '{command.Id}' was not found.");
        }

        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        _mapper.Map(command, productionSupplier);
        productionSupplier.SetUpdated();

        await _productionSupplierRepository.SaveChangesAsync(cancellationToken);

        ProductionSupplierDto productionSupplierDto = _mapper.Map<ProductionSupplierDto>(productionSupplier);

        return productionSupplierDto;
    }
}
namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Commands.CreateProductionSupplier;

public class CreateProductionSupplierHandler
{
    private readonly IProductionSupplierRepository _productionSupplierRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreateProductionSupplierHandler(IProductionSupplierRepository productionSupplierRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _productionSupplierRepository = productionSupplierRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<ProductionSupplierDto> HandleAsync(CreateProductionSupplierCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        ProductionSupplier productionSupplier = _mapper.Map<ProductionSupplier>(command);

        await _productionSupplierRepository.AddAsync(productionSupplier, cancellationToken);
        await _productionSupplierRepository.SaveChangesAsync(cancellationToken);

        ProductionSupplierDto productionSupplierDto = _mapper.Map<ProductionSupplierDto>(productionSupplier);

        return productionSupplierDto;
    }
}
namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Queries.GetProductionSuppliersByEditionId;

public class GetProductionSuppliersByEditionIdHandler
{
    private readonly IProductionSupplierRepository _productionSupplierRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public GetProductionSuppliersByEditionIdHandler(IProductionSupplierRepository productionSupplierRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _productionSupplierRepository = productionSupplierRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionSupplierDto>> HandleAsync(GetProductionSuppliersByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(query.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{query.EditionId}' was not found.");
        }

        IReadOnlyList<ProductionSupplier> productionSuppliers = await _productionSupplierRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);

        return _mapper.Map<IReadOnlyList<ProductionSupplierDto>>(productionSuppliers);
    }
}
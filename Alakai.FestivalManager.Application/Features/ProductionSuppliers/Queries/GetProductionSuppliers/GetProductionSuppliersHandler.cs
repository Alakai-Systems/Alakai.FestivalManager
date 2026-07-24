namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Queries.GetProductionSuppliers;

public class GetProductionSuppliersHandler
{
    private readonly IProductionSupplierRepository _productionSupplierRepository;
    private readonly IMapper _mapper;

    public GetProductionSuppliersHandler(IProductionSupplierRepository productionSupplierRepository, IMapper mapper)
    {
        _productionSupplierRepository = productionSupplierRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionSupplierDto>> HandleAsync(GetProductionSuppliersQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductionSupplier> productionSuppliers = await _productionSupplierRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<ProductionSupplierDto>>(productionSuppliers);
    }
}
namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Queries.GetProductionSupplierById;

public class GetProductionSupplierByIdHandler
{
    private readonly IProductionSupplierRepository _productionSupplierRepository;
    private readonly IMapper _mapper;

    public GetProductionSupplierByIdHandler(IProductionSupplierRepository productionSupplierRepository, IMapper mapper)
    {
        _productionSupplierRepository = productionSupplierRepository;
        _mapper = mapper;
    }

    public async Task<ProductionSupplierDto?> HandleAsync(GetProductionSupplierByIdQuery query, CancellationToken cancellationToken = default)
    {
        ProductionSupplier? productionSupplier = await _productionSupplierRepository.GetByIdAsync(query.Id, cancellationToken);

        if (productionSupplier is null)
        {
            return null;
        }

        return _mapper.Map<ProductionSupplierDto>(productionSupplier);
    }
}
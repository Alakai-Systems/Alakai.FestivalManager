namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Queries.GetProductionPersonById;

public class GetProductionPersonByIdHandler
{
    private readonly IProductionPersonRepository _productionPersonRepository;
    private readonly IMapper _mapper;

    public GetProductionPersonByIdHandler(IProductionPersonRepository productionPersonRepository, IMapper mapper)
    {
        _productionPersonRepository = productionPersonRepository;
        _mapper = mapper;
    }

    public async Task<ProductionPersonDto?> HandleAsync(GetProductionPersonByIdQuery query, CancellationToken cancellationToken = default)
    {
        ProductionPerson? productionPerson = await _productionPersonRepository.GetByIdAsync(query.Id, cancellationToken);

        if (productionPerson is null)
        {
            return null;
        }

        ProductionPersonDto productionPersonDto = _mapper.Map<ProductionPersonDto>(productionPerson);

        return productionPersonDto;
    }
}
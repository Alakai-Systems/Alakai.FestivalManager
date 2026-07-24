namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Queries.GetProductionPeople;

public class GetProductionPeopleHandler
{
    private readonly IProductionPersonRepository _productionPersonRepository;
    private readonly IMapper _mapper;

    public GetProductionPeopleHandler(IProductionPersonRepository productionPersonRepository, IMapper mapper)
    {
        _productionPersonRepository = productionPersonRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionPersonDto>> HandleAsync(GetProductionPeopleQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductionPerson> productionPeople = await _productionPersonRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<ProductionPersonDto> productionPeopleDtos = _mapper.Map<IReadOnlyList<ProductionPersonDto>>(productionPeople);

        return productionPeopleDtos;
    }
}
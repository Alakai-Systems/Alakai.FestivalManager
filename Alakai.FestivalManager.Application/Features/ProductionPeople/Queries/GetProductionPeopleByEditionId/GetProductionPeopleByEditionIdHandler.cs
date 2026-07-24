namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Queries.GetProductionPeopleByEditionId;

public class GetProductionPeopleByEditionIdHandler
{
    private readonly IProductionPersonRepository _productionPersonRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public GetProductionPeopleByEditionIdHandler(IProductionPersonRepository productionPersonRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _productionPersonRepository = productionPersonRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionPersonDto>> HandleAsync(GetProductionPeopleByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(query.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{query.EditionId}' was not found.");
        }

        IReadOnlyList<ProductionPerson> productionPeople = await _productionPersonRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);

        IReadOnlyList<ProductionPersonDto> productionPeopleDtos = _mapper.Map<IReadOnlyList<ProductionPersonDto>>(productionPeople);

        return productionPeopleDtos;
    }
}
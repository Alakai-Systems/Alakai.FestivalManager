namespace Alakai.FestivalManager.Application.Features.PassTypes.Queries.GetPassTypesByEditionId;

public class GetPassTypesByEditionIdHandler
{
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public GetPassTypesByEditionIdHandler(IPassTypeRepository passTypeRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _passTypeRepository = passTypeRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PassTypeDto>> HandleAsync(GetPassTypesByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(query.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{query.EditionId}' was not found.");
        }

        IReadOnlyList<PassType> passTypes = await _passTypeRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);

        IReadOnlyList<PassTypeDto> passTypeDtos = _mapper.Map<IReadOnlyList<PassTypeDto>>(passTypes);

        return passTypeDtos;
    }
}
namespace Alakai.FestivalManager.Application.Features.Levels.Queries.GetLevelsByPassTypeId;

public class GetLevelsByPassTypeIdHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IMapper _mapper;

    public GetLevelsByPassTypeIdHandler(ILevelRepository levelRepository, IPassTypeRepository passTypeRepository, IMapper mapper)
    {
        _levelRepository = levelRepository;
        _passTypeRepository = passTypeRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<LevelDto>> HandleAsync(GetLevelsByPassTypeIdQuery query, CancellationToken cancellationToken = default)
    {
        PassType? passType = await _passTypeRepository.GetByIdAsync(query.PassTypeId, cancellationToken);

        if (passType is null)
        {
            throw new NotFoundException($"Pass type with id '{query.PassTypeId}' was not found.");
        }

        IReadOnlyList<Level> levels = await _levelRepository.GetByPassTypeIdAsync(query.PassTypeId, cancellationToken);

        IReadOnlyList<LevelDto> levelDtos = _mapper.Map<IReadOnlyList<LevelDto>>(levels);

        return levelDtos;
    }
}

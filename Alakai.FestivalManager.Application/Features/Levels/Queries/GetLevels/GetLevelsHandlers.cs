namespace Alakai.FestivalManager.Application.Features.Levels.Queries.GetLevels;

public class GetLevelsHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IMapper _mapper;

    public GetLevelsHandler(ILevelRepository levelRepository, IMapper mapper)
    {
        _levelRepository = levelRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<LevelDto>> HandleAsync(GetLevelsQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Level> levels = await _levelRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<LevelDto> levelDtos = _mapper.Map<IReadOnlyList<LevelDto>>(levels);

        return levelDtos;
    }
}
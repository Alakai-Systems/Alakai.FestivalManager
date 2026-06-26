namespace Alakai.FestivalManager.Application.Features.Levels.Queries.GetLevels;

public class GetLevelsHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IMapper _mapper;

    public GetLevelsHandler(ILevelRepository levelRepository, IMapper mapper, IRegistrationRepository registrationRepository)
    {
        _levelRepository = levelRepository;
        _mapper = mapper;
        _registrationRepository = registrationRepository;
    }

    public async Task<IReadOnlyList<LevelDto>> HandleAsync(GetLevelsQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Level> levels = await _levelRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<LevelDto> levelDtos = _mapper.Map<IReadOnlyList<LevelDto>>(levels);

        foreach (LevelDto levelDto in levelDtos)
        {
            levelDto.CurrentLeaders = await _registrationRepository.CountActiveByLevelAndRoleAsync(levelDto.Id, DanceRole.Leader, cancellationToken);
            levelDto.CurrentFollowers = await _registrationRepository.CountActiveByLevelAndRoleAsync(levelDto.Id, DanceRole.Follower, cancellationToken);
            levelDto.CurrentIndividuals = await _registrationRepository.CountActiveByLevelAndRoleAsync(levelDto.Id, DanceRole.Individual, cancellationToken);
        }

        return levelDtos;
    }
}
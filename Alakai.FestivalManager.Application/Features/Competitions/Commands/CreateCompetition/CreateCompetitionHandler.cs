namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.CreateCompetition;

public class CreateCompetitionHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ICompetitionCapacityRepository _competitionCapacityRepository;
    private readonly ICompetitionLevelRepository _competitionLevelRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreateCompetitionHandler(ICompetitionRepository competitionRepository, IEditionRepository editionRepository, IMapper mapper,
        ICompetitionCapacityRepository competitionCapacityRepository, ICompetitionLevelRepository competitionLevelRepository)
    {
        _competitionRepository = competitionRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
        _competitionCapacityRepository = competitionCapacityRepository;
        _competitionLevelRepository = competitionLevelRepository;
    }

    public async Task<CompetitionDto> HandleAsync(CreateCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        bool exists = await _competitionRepository.ExistsByEditionAndNameAsync(command.EditionId, command.Name, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"A competition named '{command.Name}' already exists for this edition.");
        }

        Competition competition = _mapper.Map<Competition>(command);
        competition.IsActive = true;

        await _competitionRepository.AddAsync(competition, cancellationToken);
        await _competitionRepository.SaveChangesAsync(cancellationToken);

        // Create the levels (free-text names) this brand-new competition uses, if any.
        Dictionary<string, Guid> levelNameToId = new(StringComparer.OrdinalIgnoreCase);
        int sortOrder = 1;

        foreach (string levelName in command.LevelNames.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            CompetitionLevel level = new()
            {
                CompetitionId = competition.Id,
                Name = levelName,
                SortOrder = sortOrder++,
                IsActive = true
            };

            await _competitionLevelRepository.AddAsync(level, cancellationToken);
            levelNameToId[levelName] = level.Id;
        }

        await _competitionLevelRepository.SaveChangesAsync(cancellationToken);

        List<CompetitionCapacity> capacities = command.Capacities
            .Select(capacityCommand => new CompetitionCapacity
            {
                CompetitionId = competition.Id,
                CompetitionLevelId = !string.IsNullOrWhiteSpace(capacityCommand.LevelName) && levelNameToId.TryGetValue(capacityCommand.LevelName, out Guid levelId)
                    ? levelId
                    : null,
                DanceRole = capacityCommand.DanceRole,
                Capacity = capacityCommand.Capacity,
                SortOrder = capacityCommand.SortOrder,
                IsActive = true
            })
            .ToList();

        await _competitionCapacityRepository.AddRangeAsync(capacities, cancellationToken);
        await _competitionCapacityRepository.SaveChangesAsync(cancellationToken);

        Competition? reloaded = await _competitionRepository.GetByIdAsync(competition.Id, cancellationToken);

        return _mapper.Map<CompetitionDto>(reloaded ?? competition);
    }
}

namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.UpdateCompetition;

public class UpdateCompetitionHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ICompetitionCapacityRepository _competitionCapacityRepository;
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly ICompetitionLevelRepository _competitionLevelRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdateCompetitionHandler(ICompetitionRepository competitionRepository, IEditionRepository editionRepository, IMapper mapper,
        ICompetitionCapacityRepository competitionCapacityRepository, ICompetitionEntryRepository competitionEntryRepository,
        ICompetitionLevelRepository competitionLevelRepository)
    {
        _competitionRepository = competitionRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
        _competitionCapacityRepository = competitionCapacityRepository;
        _competitionEntryRepository = competitionEntryRepository;
        _competitionLevelRepository = competitionLevelRepository;
    }

    public async Task<CompetitionDto> HandleAsync(UpdateCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        Competition? competition = await _competitionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (competition is null)
            throw new NotFoundException($"Competition with id '{command.Id}' was not found.");

        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");

        _mapper.Map(command, competition);
        competition.SetUpdated();

        // --- Sync levels: create any new ones by name. Existing levels are never
        // auto-deleted here (that would risk orphaning historical capacities/entries);
        // removing a level is a deliberate separate action, out of scope for this save. ---
        IReadOnlyList<CompetitionLevel> existingLevels = await _competitionLevelRepository.GetByCompetitionIdAsync(competition.Id, cancellationToken);
        Dictionary<string, Guid> levelNameToId = existingLevels.ToDictionary(l => l.Name, l => l.Id, StringComparer.OrdinalIgnoreCase);

        int nextSortOrder = existingLevels.Count + 1;

        foreach (string levelName in command.LevelNames.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!levelNameToId.ContainsKey(levelName))
            {
                CompetitionLevel newLevel = new()
                {
                    CompetitionId = competition.Id,
                    Name = levelName,
                    SortOrder = nextSortOrder++,
                    IsActive = true
                };

                await _competitionLevelRepository.AddAsync(newLevel, cancellationToken);
                levelNameToId[levelName] = newLevel.Id;
            }
        }

        await _competitionLevelRepository.SaveChangesAsync(cancellationToken);

        Guid? ResolveLevelId(string? levelName) =>
            !string.IsNullOrWhiteSpace(levelName) && levelNameToId.TryGetValue(levelName, out Guid id) ? id : null;

        IReadOnlyList<CompetitionCapacity> existingCapacities =
            await _competitionCapacityRepository.GetByCompetitionIdAsync(competition.Id, cancellationToken);

        foreach (CompetitionCapacity existing in existingCapacities)
        {
            UpdateCompetitionCapacityCommand? incoming = command.Capacities
                .FirstOrDefault(c => ResolveLevelId(c.LevelName) == existing.CompetitionLevelId && c.DanceRole == existing.DanceRole);

            if (incoming is not null)
            {
                existing.Capacity = incoming.Capacity;
                existing.SortOrder = incoming.SortOrder;
                existing.IsActive = true;
            }
            else
            {
                bool hasEntries = await _competitionEntryRepository.ExistsByCapacityIdAsync(existing.Id, cancellationToken);

                if (hasEntries)
                {
                    existing.IsActive = false;
                }
                else
                {
                    _competitionCapacityRepository.Delete(existing);
                }
            }
        }

        foreach (UpdateCompetitionCapacityCommand incoming in command.Capacities)
        {
            Guid? incomingLevelId = ResolveLevelId(incoming.LevelName);

            bool alreadyExists = existingCapacities.Any(e =>
                e.CompetitionLevelId == incomingLevelId && e.DanceRole == incoming.DanceRole);

            if (!alreadyExists)
            {
                CompetitionCapacity newCap = new()
                {
                    CompetitionId = competition.Id,
                    CompetitionLevelId = incomingLevelId,
                    DanceRole = incoming.DanceRole,
                    Capacity = incoming.Capacity,
                    SortOrder = incoming.SortOrder,
                    IsActive = true
                };

                await _competitionCapacityRepository.AddAsync(newCap, cancellationToken);
            }
        }

        await _competitionRepository.SaveChangesAsync(cancellationToken);

        Competition? reloaded = await _competitionRepository.GetByIdAsync(competition.Id, cancellationToken);

        return _mapper.Map<CompetitionDto>(reloaded ?? competition);
    }
}

namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.UpdateCompetition;

public class UpdateCompetitionHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ICompetitionCapacityRepository _competitionCapacityRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdateCompetitionHandler(ICompetitionRepository competitionRepository, IEditionRepository editionRepository, IMapper mapper, ICompetitionCapacityRepository competitionCapacityRepository)
    {
        _competitionRepository = competitionRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
        _competitionCapacityRepository = competitionCapacityRepository;
    }

    public async Task<CompetitionDto> HandleAsync(UpdateCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        Competition? competition = await _competitionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (competition is null)
        {
            throw new NotFoundException($"Competition with id '{command.Id}' was not found.");
        }

        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        _mapper.Map(command, competition);

        competition.SetUpdated();

        IReadOnlyList<CompetitionCapacity> existingCapacities =
            await _competitionCapacityRepository.GetByCompetitionIdAsync(
                competition.Id,
                cancellationToken);

        foreach (CompetitionCapacity capacity in existingCapacities)
        {
            _competitionCapacityRepository.Delete(capacity);
        }

        foreach (UpdateCompetitionCapacityCommand capacityCommand in command.Capacities)
        {
            CompetitionCapacity capacity = new()
            {
                CompetitionId = competition.Id,
                MixAndMatchLevel = capacityCommand.MixAndMatchLevel,
                DanceRole = capacityCommand.DanceRole,
                Capacity = capacityCommand.Capacity,
                SortOrder = capacityCommand.SortOrder,
                IsActive = true
            };

            await _competitionCapacityRepository.AddAsync(
                capacity,
                cancellationToken);
        }

        await _competitionRepository.SaveChangesAsync(cancellationToken);

        CompetitionDto dto = _mapper.Map<CompetitionDto>(competition);

        return dto;
    }
}

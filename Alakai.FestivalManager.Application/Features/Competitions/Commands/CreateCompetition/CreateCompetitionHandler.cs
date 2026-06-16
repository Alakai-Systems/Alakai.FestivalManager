namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.CreateCompetition;

public class CreateCompetitionHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ICompetitionCapacityRepository _competitionCapacityRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreateCompetitionHandler(ICompetitionRepository competitionRepository, IEditionRepository editionRepository, IMapper mapper, ICompetitionCapacityRepository competitionCapacityRepository)
    {
        _competitionRepository = competitionRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
        _competitionCapacityRepository = competitionCapacityRepository;
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

        List<CompetitionCapacity> capacities = command.Capacities
            .Select(capacityCommand => new CompetitionCapacity
            {
                CompetitionId = competition.Id,
                MixAndMatchLevel = capacityCommand.MixAndMatchLevel,
                DanceRole = capacityCommand.DanceRole,
                Capacity = capacityCommand.Capacity,
                SortOrder = capacityCommand.SortOrder,
                IsActive = true
            })
            .ToList();

        await _competitionCapacityRepository.AddRangeAsync(capacities, cancellationToken);
        await _competitionCapacityRepository.SaveChangesAsync(cancellationToken);

        CompetitionDto dto = _mapper.Map<CompetitionDto>(competition);

        return dto;
    }
}

namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.CreateCompetition;

public class CreateCompetitionHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreateCompetitionHandler(ICompetitionRepository competitionRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _competitionRepository = competitionRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
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

        CompetitionDto dto = _mapper.Map<CompetitionDto>(competition);

        return dto;
    }
}

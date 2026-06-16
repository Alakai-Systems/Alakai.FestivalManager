using Alakai.FestivalManager.Application.Features.Competitions.DTOs;

namespace Alakai.FestivalManager.Application.Features.Competitions.Commands.UpdateCompetition;

public class UpdateCompetitionHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdateCompetitionHandler(ICompetitionRepository competitionRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _competitionRepository = competitionRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
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
        await _competitionRepository.SaveChangesAsync(cancellationToken);

        CompetitionDto dto = _mapper.Map<CompetitionDto>(competition);

        return dto;
    }
}

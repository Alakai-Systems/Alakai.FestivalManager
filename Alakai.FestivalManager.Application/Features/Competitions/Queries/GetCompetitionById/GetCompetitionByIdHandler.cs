using Alakai.FestivalManager.Application.Features.Competitions.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Competitions.Queries.GetCompetitionById;

public class GetCompetitionByIdHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IMapper _mapper;

    public GetCompetitionByIdHandler(ICompetitionRepository competitionRepository, IMapper mapper)
    {
        _competitionRepository = competitionRepository;
        _mapper = mapper;
    }

    public async Task<CompetitionDto> HandleAsync(GetCompetitionByIdQuery query, CancellationToken cancellationToken = default)
    {
        Competition? competition = await _competitionRepository.GetByIdAsync(query.Id, cancellationToken);

        if (competition is null)
        {
            throw new NotFoundException($"Competition with id '{query.Id}' was not found.");
        }

        return _mapper.Map<CompetitionDto>(competition);
    }
}

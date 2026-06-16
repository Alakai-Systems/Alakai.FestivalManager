namespace Alakai.FestivalManager.Application.Features.Competitions.Queries.GetCompetitionsByEditionId;

public class GetCompetitionsByEditionIdHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IMapper _mapper;

    public GetCompetitionsByEditionIdHandler(ICompetitionRepository competitionRepository, IMapper mapper)
    {
        _competitionRepository = competitionRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompetitionDto>> HandleAsync(GetCompetitionsByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Competition> competitions = await _competitionRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CompetitionDto>>(competitions);
    }
}

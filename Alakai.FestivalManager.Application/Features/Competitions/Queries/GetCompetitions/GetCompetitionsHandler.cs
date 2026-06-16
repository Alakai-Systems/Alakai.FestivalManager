namespace Alakai.FestivalManager.Application.Features.Competitions.Queries.GetCompetitions;

public class GetCompetitionsHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IMapper _mapper;

    public GetCompetitionsHandler(ICompetitionRepository competitionRepository, IMapper mapper)
    {
        _competitionRepository = competitionRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompetitionDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Competition> competitions = await _competitionRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CompetitionDto>>(competitions);
    }
}

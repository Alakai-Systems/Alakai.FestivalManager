namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Queries.GetCompetitionEntriesByCompetitionId;

public class GetCompetitionEntriesByCompetitionIdHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IMapper _mapper;

    public GetCompetitionEntriesByCompetitionIdHandler(ICompetitionEntryRepository competitionEntryRepository, IMapper mapper)
    {
        _competitionEntryRepository = competitionEntryRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompetitionEntryDto>> HandleAsync(GetCompetitionEntriesByCompetitionIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntry> entries = await _competitionEntryRepository.GetByCompetitionIdAsync(query.CompetitionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CompetitionEntryDto>>(entries);
    }
}

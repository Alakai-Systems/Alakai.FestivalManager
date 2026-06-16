namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Queries.GetCompetitionEntries;

public class GetCompetitionEntriesHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IMapper _mapper;

    public GetCompetitionEntriesHandler(ICompetitionEntryRepository competitionEntryRepository, IMapper mapper)
    {
        _competitionEntryRepository = competitionEntryRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompetitionEntryDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntry> entries = await _competitionEntryRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CompetitionEntryDto>>(entries);
    }
}

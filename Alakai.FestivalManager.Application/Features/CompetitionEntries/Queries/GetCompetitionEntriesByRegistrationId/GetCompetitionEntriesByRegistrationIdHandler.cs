namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Queries.GetCompetitionEntriesByRegistrationId;

public class GetCompetitionEntriesByRegistrationIdHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IMapper _mapper;

    public GetCompetitionEntriesByRegistrationIdHandler(ICompetitionEntryRepository competitionEntryRepository, IMapper mapper)
    {
        _competitionEntryRepository = competitionEntryRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompetitionEntryDto>> HandleAsync(GetCompetitionEntriesByRegistrationIdQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntry> entries = await _competitionEntryRepository.GetByRegistrationIdAsync(query.RegistrationId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CompetitionEntryDto>>(entries);
    }
}

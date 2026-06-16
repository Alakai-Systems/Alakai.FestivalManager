namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Queries.GetCompetitionEntryById;

public class GetCompetitionEntryByIdHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IMapper _mapper;

    public GetCompetitionEntryByIdHandler(ICompetitionEntryRepository competitionEntryRepository, IMapper mapper)
    {
        _competitionEntryRepository = competitionEntryRepository;
        _mapper = mapper;
    }

    public async Task<CompetitionEntryDto> HandleAsync(GetCompetitionEntryByIdQuery query, CancellationToken cancellationToken = default)
    {
        CompetitionEntry? entry = await _competitionEntryRepository.GetByIdAsync(query.Id, cancellationToken);

        if (entry is null)
        {
            throw new NotFoundException($"Competition entry with id '{query.Id}' was not found.");
        }

        return _mapper.Map<CompetitionEntryDto>(entry);
    }
}

namespace Alakai.FestivalManager.Application.Features.Competitions.Queries.GetCompetitions;

public class GetCompetitionsHandler
{
    private readonly ICompetitionRepository _competitionRepository;
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IMapper _mapper;

    public GetCompetitionsHandler(ICompetitionRepository competitionRepository, ICompetitionEntryRepository competitionEntryRepository, IMapper mapper)
    {
        _competitionRepository = competitionRepository;
        _competitionEntryRepository = competitionEntryRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CompetitionDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Competition> competitions = await _competitionRepository.GetAllAsync(cancellationToken);

        List<CompetitionDto> dtos = _mapper.Map<List<CompetitionDto>>(competitions);

        foreach (CompetitionDto competitionDto in dtos)
        {
            foreach (CompetitionCapacityDto capacityDto in competitionDto.Capacities)
            {
                int current = await _competitionEntryRepository
                    .CountActiveByCapacityIdAsync(capacityDto.Id, cancellationToken);

                capacityDto.Current = current;
            }
        }

        return dtos;
    }
}


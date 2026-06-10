namespace Alakai.FestivalManager.Application.Features.Festivals.Queries.GetFestivals;

public class GetFestivalsHandler
{
    private readonly IFestivalRepository _festivalRepository;
    private readonly IMapper _mapper;

    public GetFestivalsHandler(IFestivalRepository festivalRepository, IMapper mapper)
    {
        _festivalRepository = festivalRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<FestivalDto>> HandleAsync(GetFestivalsQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Festival> festivals = await _festivalRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<FestivalDto> festivalDtos = _mapper.Map<IReadOnlyList<FestivalDto>>(festivals);

        return festivalDtos;
    }
}
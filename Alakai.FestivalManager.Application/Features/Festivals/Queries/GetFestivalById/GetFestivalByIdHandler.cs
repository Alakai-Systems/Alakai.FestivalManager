namespace Alakai.FestivalManager.Application.Features.Festivals.Queries.GetFestivalById;

public class GetFestivalByIdHandler
{
    private readonly IFestivalRepository _festivalRepository;
    private readonly IMapper _mapper;

    public GetFestivalByIdHandler(IFestivalRepository festivalRepository, IMapper mapper)
    {
        _festivalRepository = festivalRepository;
        _mapper = mapper;
    }

    public async Task<FestivalDto?> HandleAsync(GetFestivalByIdQuery query, CancellationToken cancellationToken = default)
    {
        Festival? festival = await _festivalRepository.GetByIdAsync(query.Id, cancellationToken);

        if (festival is null)
        {
            return null;
        }

        FestivalDto festivalDto = _mapper.Map<FestivalDto>(festival);

        return festivalDto;
    }
}

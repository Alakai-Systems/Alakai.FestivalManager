namespace Alakai.FestivalManager.Application.Features.Editions.Queries.GetEditions;

public class GetEditionsHandler
{
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public GetEditionsHandler(IEditionRepository editionRepository, IMapper mapper)
    {
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EditionDto>> HandleAsync(GetEditionsQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Edition> editions = await _editionRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<EditionDto> editionDtoList = _mapper.Map<IReadOnlyList<EditionDto>>(editions);

        return editionDtoList;
    }
}

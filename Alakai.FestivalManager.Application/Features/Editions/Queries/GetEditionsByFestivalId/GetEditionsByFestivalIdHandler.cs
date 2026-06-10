namespace Alakai.FestivalManager.Application.Features.Editions.Queries.GetEditionsByFestivalId;

public class GetEditionsByFestivalIdHandler
{
    private readonly IEditionRepository _editionRepository;
    private readonly IFestivalRepository _festivalRepository;
    private readonly IMapper _mapper;

    public GetEditionsByFestivalIdHandler(IEditionRepository editionRepository, IFestivalRepository festivalRepository, IMapper mapper)
    {
        _editionRepository = editionRepository;
        _festivalRepository = festivalRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EditionDto>> HandleAsync(GetEditionsByFestivalIdQuery query, CancellationToken cancellationToken = default)
    {
        bool festivalExists = await _festivalRepository.GetByIdAsync(query.FestivalId, cancellationToken) is not null;

        if (!festivalExists)
        {
            throw new Common.Exceptions.NotFoundException($"Festival with id '{query.FestivalId}' was not found.");
        }

        IReadOnlyList<Edition> editions = await _editionRepository.GetByFestivalIdAsync(query.FestivalId, cancellationToken);

        IReadOnlyList<EditionDto> editionDtos = _mapper.Map<IReadOnlyList<EditionDto>>(editions);

        return editionDtos;
    }
}
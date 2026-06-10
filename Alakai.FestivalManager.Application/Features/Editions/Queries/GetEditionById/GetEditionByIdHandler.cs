namespace Alakai.FestivalManager.Application.Features.Editions.Queries.GetEditionById;
public class GetEditionByIdHandler
{
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public GetEditionByIdHandler(IEditionRepository editionRepository, IMapper mapper)
    {
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<EditionDto?> HandleAsync(GetEditionByIdQuery query, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(query.Id, cancellationToken);

        if (edition is null)
        {
            return null;
        }

        EditionDto editionDto = _mapper.Map<EditionDto>(edition);

        return editionDto;
    }
}
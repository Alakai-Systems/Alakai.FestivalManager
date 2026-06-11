namespace Alakai.FestivalManager.Application.Features.Levels.Queries.GetLevelById;

public class GetLevelByIdHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IMapper _mapper;

    public GetLevelByIdHandler(ILevelRepository levelRepository, IMapper mapper)
    {
        _levelRepository = levelRepository;
        _mapper = mapper;
    }

    public async Task<LevelDto?> HandleAsync(GetLevelByIdQuery query, CancellationToken cancellationToken = default)
    {
        Level? level = await _levelRepository.GetByIdAsync(query.Id, cancellationToken);

        if (level is null)
        {
            return null;
        }

        LevelDto levelDto = _mapper.Map<LevelDto>(level);

        return levelDto;
    }
}
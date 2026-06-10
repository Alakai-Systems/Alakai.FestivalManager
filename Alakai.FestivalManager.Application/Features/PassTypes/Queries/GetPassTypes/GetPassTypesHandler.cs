namespace Alakai.FestivalManager.Application.Features.PassTypes.Queries.GetPassTypes;

public class GetPassTypesHandler
{
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IMapper _mapper;

    public GetPassTypesHandler(IPassTypeRepository passTypeRepository, IMapper mapper)
    {
        _passTypeRepository = passTypeRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PassTypeDto>> HandleAsync(GetPassTypesQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PassType> passTypes = await _passTypeRepository.GetAllAsync(cancellationToken);

        IReadOnlyList<PassTypeDto> passTypeDtos = _mapper.Map<IReadOnlyList<PassTypeDto>>(passTypes);

        return passTypeDtos;
    }
}

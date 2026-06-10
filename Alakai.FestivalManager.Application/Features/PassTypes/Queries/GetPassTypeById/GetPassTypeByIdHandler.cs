namespace Alakai.FestivalManager.Application.Features.PassTypes.Queries.GetPassTypeById;

public class GetPassTypeByIdHandler
{
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IMapper _mapper;

    public GetPassTypeByIdHandler(IPassTypeRepository passTypeRepository, IMapper mapper)
    {
        _passTypeRepository = passTypeRepository;
        _mapper = mapper;
    }

    public async Task<PassTypeDto?> HandleAsync(GetPassTypeByIdQuery query, CancellationToken cancellationToken = default)
    {
        PassType? passType = await _passTypeRepository.GetByIdAsync(query.Id, cancellationToken);

        if (passType is null)
        {
            return null;
        }

        PassTypeDto passTypeDto = _mapper.Map<PassTypeDto>(passType);

        return passTypeDto;
    }
}
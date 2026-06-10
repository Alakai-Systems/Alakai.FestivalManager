namespace Alakai.FestivalManager.Application.Features.PassTypes.Commands.UpdatePassType;

public class UpdatePassTypeHandler
{
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdatePassTypeHandler(IPassTypeRepository passTypeRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _passTypeRepository = passTypeRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<PassTypeDto> HandleAsync(UpdatePassTypeCommand command, CancellationToken cancellationToken = default)
    {
        PassType? passType = await _passTypeRepository.GetByIdAsync(command.Id, cancellationToken);

        if (passType is null)
        {
            throw new NotFoundException($"Pass type with id '{command.Id}' was not found.");
        }

        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        _mapper.Map(command, passType);
        passType.SetUpdated();

        await _passTypeRepository.SaveChangesAsync(cancellationToken);

        PassTypeDto passTypeDto = _mapper.Map<PassTypeDto>(passType);

        return passTypeDto;
    }
}
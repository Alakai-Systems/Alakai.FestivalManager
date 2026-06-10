namespace Alakai.FestivalManager.Application.Features.PassTypes.Commands.CreatePassType;

public class CreatePassTypeHandler
{
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreatePassTypeHandler(IPassTypeRepository passTypeRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _passTypeRepository = passTypeRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<PassTypeDto> HandleAsync(CreatePassTypeCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        bool exists = await _passTypeRepository.ExistsByEditionAndNameAsync(command.EditionId, command.Name, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"Pass type '{command.Name}' already exists for this edition.");
        }

        PassType passType = _mapper.Map<PassType>(command);

        await _passTypeRepository.AddAsync(passType, cancellationToken);
        await _passTypeRepository.SaveChangesAsync(cancellationToken);

        PassTypeDto passTypeDto = _mapper.Map<PassTypeDto>(passType);

        return passTypeDto;
    }
}
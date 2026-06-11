namespace Alakai.FestivalManager.Application.Features.Levels.Commands.CreateLevel;

public class CreateLevelHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IMapper _mapper;

    public CreateLevelHandler(ILevelRepository levelRepository, IPassTypeRepository passTypeRepository, IMapper mapper)
    {
        _levelRepository = levelRepository;
        _passTypeRepository = passTypeRepository;
        _mapper = mapper;
    }

    public async Task<LevelDto> HandleAsync(CreateLevelCommand command, CancellationToken cancellationToken = default)
    {
        PassType? passType = await _passTypeRepository.GetByIdAsync(command.PassTypeId, cancellationToken);

        if (passType is null)
        {
            throw new NotFoundException($"Pass type with id '{command.PassTypeId}' was not found.");
        }

        bool exists = await _levelRepository.ExistsByPassTypeAndNameAsync(command.PassTypeId, command.Name, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"Level '{command.Name}' already exists for this pass type.");
        }

        Level level = _mapper.Map<Level>(command);

        await _levelRepository.AddAsync(level, cancellationToken);
        await _levelRepository.SaveChangesAsync(cancellationToken);

        LevelDto levelDto = _mapper.Map<LevelDto>(level);

        return levelDto;
    }
}
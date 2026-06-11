namespace Alakai.FestivalManager.Application.Features.Levels.Commands.UpdateLevel;
public class UpdateLevelHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IMapper _mapper;

    public UpdateLevelHandler(ILevelRepository levelRepository, IPassTypeRepository passTypeRepository, IMapper mapper)
    {
        _levelRepository = levelRepository;
        _passTypeRepository = passTypeRepository;
        _mapper = mapper;
    }

    public async Task<LevelDto> HandleAsync(UpdateLevelCommand command, CancellationToken cancellationToken = default)
    {
        Level? level = await _levelRepository.GetByIdAsync(command.Id, cancellationToken);

        if (level is null)
        {
            throw new NotFoundException($"Level with id '{command.Id}' was not found.");
        }

        PassType? passType = await _passTypeRepository.GetByIdAsync(command.PassTypeId, cancellationToken);

        if (passType is null)
        {
            throw new NotFoundException($"Pass type with id '{command.PassTypeId}' was not found.");
        }

        _mapper.Map(command, level);

        level.SetUpdated();

        await _levelRepository.SaveChangesAsync(cancellationToken);

        LevelDto levelDto = _mapper.Map<LevelDto>(level);

        return levelDto;
    }
}
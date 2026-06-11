namespace Alakai.FestivalManager.Application.Features.Levels.Commands.DeleteLevel;

public class DeleteLevelHandler
{
    private readonly ILevelRepository _levelRepository;

    public DeleteLevelHandler(ILevelRepository levelRepository)
    {
        _levelRepository = levelRepository;
    }

    public async Task<Guid> HandleAsync(DeleteLevelCommand command, CancellationToken cancellationToken = default)
    {
        Level? level = await _levelRepository.GetByIdAsync(command.Id, cancellationToken);

        if (level is null)
        {
            throw new NotFoundException($"Level with id '{command.Id}' was not found.");
        }

        _levelRepository.Delete(level);

        await _levelRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}
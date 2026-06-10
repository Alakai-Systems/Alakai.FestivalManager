namespace Alakai.FestivalManager.Application.Features.PassTypes.Commands.DeletePassType;

public class DeletePassTypeHandler
{
    private readonly IPassTypeRepository _passTypeRepository;

    public DeletePassTypeHandler(IPassTypeRepository passTypeRepository)
    {
        _passTypeRepository = passTypeRepository;
    }

    public async Task<Guid> HandleAsync(DeletePassTypeCommand command, CancellationToken cancellationToken = default)
    {
        PassType? passType = await _passTypeRepository.GetByIdAsync(command.Id, cancellationToken);

        if (passType is null)
        {
            throw new NotFoundException($"Pass type with id '{command.Id}' was not found.");
        }

        _passTypeRepository.Delete(passType);

        await _passTypeRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}
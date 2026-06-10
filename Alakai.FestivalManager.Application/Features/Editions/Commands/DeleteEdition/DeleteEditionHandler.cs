namespace Alakai.FestivalManager.Application.Features.Editions.Commands.DeleteEdition;

public class DeleteEditionHandler
{
    private readonly IEditionRepository _editionRepository;

    public DeleteEditionHandler(IEditionRepository editionRepository)
    {
        _editionRepository = editionRepository;
    }

    public async Task<Guid> HandleAsync(DeleteEditionCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.Id}' was not found.");
        }

        _editionRepository.Delete(edition);

        await _editionRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}
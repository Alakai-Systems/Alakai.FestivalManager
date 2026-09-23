namespace Alakai.FestivalManager.Application.Features.Editions.Commands.RemoveEditionSchedule;

public class RemoveEditionScheduleHandler
{
    private readonly IEditionRepository _editionRepository;
    private readonly IFileStorageService _fileStorageService;

    public RemoveEditionScheduleHandler(IEditionRepository editionRepository, IFileStorageService fileStorageService)
    {
        _editionRepository = editionRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task HandleAsync(RemoveEditionScheduleCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        string? previousUrl = edition.ScheduleUrl;

        edition.ScheduleUrl = null;
        edition.SetUpdated();

        await _editionRepository.SaveChangesAsync(cancellationToken);

        await _fileStorageService.DeleteAsync(previousUrl, cancellationToken);
    }
}
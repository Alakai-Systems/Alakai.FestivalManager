namespace Alakai.FestivalManager.Application.Features.Editions.Commands.SetEditionSchedule;

public class SetEditionScheduleHandler
{
    private readonly IEditionRepository _editionRepository;
    private readonly IFileStorageService _fileStorageService;

    public SetEditionScheduleHandler(IEditionRepository editionRepository, IFileStorageService fileStorageService)
    {
        _editionRepository = editionRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<string> HandleAsync(SetEditionScheduleCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        string? previousUrl = edition.ScheduleUrl;

        string url = await _fileStorageService.SaveFileAsync(command.Content, command.FileName, cancellationToken);

        edition.ScheduleUrl = url;
        edition.SetUpdated();

        await _editionRepository.SaveChangesAsync(cancellationToken);

        // Best-effort: si habia un PDF anterior, se borra despues de confirmar
        // el cambio en BD (nunca lanza, ver IFileStorageService.DeleteAsync).
        await _fileStorageService.DeleteAsync(previousUrl, cancellationToken);

        return url;
    }
}
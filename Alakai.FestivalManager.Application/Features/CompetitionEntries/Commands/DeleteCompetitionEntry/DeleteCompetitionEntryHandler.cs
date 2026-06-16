namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Commands.DeleteCompetitionEntry;

public class DeleteCompetitionEntryHandler
{
    private readonly ICompetitionEntryRepository _competitionEntryRepository;

    public DeleteCompetitionEntryHandler(ICompetitionEntryRepository competitionEntryRepository)
    {
        _competitionEntryRepository = competitionEntryRepository;
    }

    public async Task HandleAsync(DeleteCompetitionEntryCommand command, CancellationToken cancellationToken = default)
    {
        CompetitionEntry? entry = await _competitionEntryRepository.GetByIdAsync(command.Id, cancellationToken);

        if (entry is null)
        {
            throw new NotFoundException($"Competition entry with id '{command.Id}' was not found.");
        }

        entry.IsActive = false;
        entry.Status = CompetitionEntryStatus.Cancelled;
        entry.CancelledAt = DateTime.UtcNow;

        _competitionEntryRepository.Update(entry);
        await _competitionEntryRepository.SaveChangesAsync(cancellationToken);
    }
}

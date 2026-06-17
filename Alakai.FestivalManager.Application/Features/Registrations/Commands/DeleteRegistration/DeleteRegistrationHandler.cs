namespace Alakai.FestivalManager.Application.Features.Registrations.Commands.DeleteRegistration;

public class DeleteRegistrationHandler
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IEmailLogRepository _emailLogRepository;

    public DeleteRegistrationHandler(IRegistrationRepository registrationRepository, ICompetitionEntryRepository competitionEntryRepository, IEmailLogRepository emailLogRepository)
    {
        _registrationRepository = registrationRepository;
        _competitionEntryRepository = competitionEntryRepository;
        _emailLogRepository = emailLogRepository;
    }

    public async Task<Guid> HandleAsync(DeleteRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        Registration? existing = await _registrationRepository.GetByIdAsync(command.Id, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"Registration with id '{command.Id}' was not found.");
        }

        IReadOnlyList<CompetitionEntry> competitionEntries = await _competitionEntryRepository.GetByRegistrationIdAsync(command.Id, cancellationToken);

        foreach (CompetitionEntry competitionEntry in competitionEntries)
        {
            _competitionEntryRepository.Delete(competitionEntry);
        }

        IReadOnlyList<EmailLog> emailLogs = await _emailLogRepository.GetByRegistrationIdAsync(command.Id, cancellationToken);

        foreach (EmailLog emailLog in emailLogs)
        {
            _emailLogRepository.Delete(emailLog);
        }

        _registrationRepository.Delete(existing);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}

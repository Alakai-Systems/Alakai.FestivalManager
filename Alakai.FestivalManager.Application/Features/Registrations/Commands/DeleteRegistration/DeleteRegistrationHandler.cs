using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Registrations.Commands.DeleteRegistration;

public class DeleteRegistrationHandler
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IDiscountCodeRepository _discountCodeRepository;

    public DeleteRegistrationHandler(IRegistrationRepository registrationRepository, ICompetitionEntryRepository competitionEntryRepository, 
        IEmailLogRepository emailLogRepository, IDiscountCodeRepository discountCodeRepository)
    {
        _registrationRepository = registrationRepository;
        _competitionEntryRepository = competitionEntryRepository;
        _emailLogRepository = emailLogRepository;
        _discountCodeRepository = discountCodeRepository;
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

        Guid? codeId = existing.DiscountCodeId;

        existing.Status = RegistrationStatus.Cancelled;
        existing.CancelledAt = DateTime.UtcNow;
        existing.IsActive = false;

        _registrationRepository.Delete(existing);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        if (codeId.HasValue)
        {
            int uses = await _registrationRepository.CountByDiscountCodeAsync(codeId.Value, cancellationToken);
            DiscountCode? code = await _discountCodeRepository.GetByIdAsync(codeId.Value, cancellationToken);

            if (code is not null)
            {
                code.CurrentUses = uses;

                if (code.ActivationType != DiscountActivationType.Immediate && uses == 0)
                {
                    _discountCodeRepository.Delete(code);
                }

                await _discountCodeRepository.SaveChangesAsync(cancellationToken);
            }
        }

        return command.Id;
    }
}

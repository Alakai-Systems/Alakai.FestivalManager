using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Registrations.Commands.DeleteRegistration;

public class DeleteRegistrationHandler
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IDiscountCodeRepository _discountCodeRepository;
    private readonly IAccommodationReservationRepository _accommodationReservationRepository;
    private readonly IBusReservationRepository _busReservationRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IEmailNotificationService _emailNotificationService;

    public DeleteRegistrationHandler(IRegistrationRepository registrationRepository, ICompetitionEntryRepository competitionEntryRepository,
        IEmailLogRepository emailLogRepository, IDiscountCodeRepository discountCodeRepository,
        IAccommodationReservationRepository accommodationReservationRepository, IBusReservationRepository busReservationRepository,
        IInvoiceRepository invoiceRepository, IEmailNotificationService emailNotificationService)
    {
        _registrationRepository = registrationRepository;
        _competitionEntryRepository = competitionEntryRepository;
        _emailLogRepository = emailLogRepository;
        _discountCodeRepository = discountCodeRepository;
        _accommodationReservationRepository = accommodationReservationRepository;
        _busReservationRepository = busReservationRepository;
        _invoiceRepository = invoiceRepository;
        _emailNotificationService = emailNotificationService;
    }

    public async Task<Guid> HandleAsync(DeleteRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        Registration? existing = await _registrationRepository.GetByIdAsync(command.Id, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"Registration with id '{command.Id}' was not found.");
        }

        // Null out PartnerRegistrationId in any partner competition entries
        IReadOnlyList<CompetitionEntry> partnerEntries = await _competitionEntryRepository.GetByPartnerRegistrationIdAsync(command.Id, cancellationToken);

        foreach (CompetitionEntry partnerEntry in partnerEntries)
        {
            partnerEntry.PartnerRegistrationId = null;
            _competitionEntryRepository.Update(partnerEntry);
        }

        IReadOnlyList<CompetitionEntry> competitionEntries = await _competitionEntryRepository.GetByRegistrationIdAsync(command.Id, cancellationToken);

        foreach (CompetitionEntry competitionEntry in competitionEntries)
        {
            _competitionEntryRepository.Delete(competitionEntry);
        }

        // Handle accommodation reservation: transfer responsibility or delete
        AccommodationReservation? accommodationReservation = await _accommodationReservationRepository.GetByResponsibleRegistrationIdTrackedAsync(command.Id, cancellationToken);

        if (accommodationReservation is not null)
        {
            AccommodationReservationOccupant? newResponsibleOccupant = accommodationReservation.Occupants
                .FirstOrDefault(o => o.RegistrationId.HasValue && o.RegistrationId != command.Id);

            if (newResponsibleOccupant is not null)
            {
                accommodationReservation.ResponsibleRegistrationId = newResponsibleOccupant.RegistrationId!.Value;
                newResponsibleOccupant.IsResponsible = true;

                AccommodationReservationOccupant? oldOccupant = accommodationReservation.Occupants
                    .FirstOrDefault(o => o.RegistrationId == command.Id);

                if (oldOccupant is not null)
                {
                    oldOccupant.IsResponsible = false;
                }
            }
            else
            {
                _accommodationReservationRepository.Delete(accommodationReservation);
            }
        }

        // Delete bus reservations
        IReadOnlyList<BusReservation> busReservations = await _busReservationRepository.GetByRegistrationIdAsync(command.Id, cancellationToken);

        foreach (BusReservation busReservation in busReservations)
        {
            _busReservationRepository.Delete(busReservation);
        }

        // Delete invoice if present
        Invoice? invoice = await _invoiceRepository.GetByRegistrationIdAsync(command.Id, cancellationToken);

        if (invoice is not null)
        {
            _invoiceRepository.Delete(invoice);
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

        // Send email to new accommodation responsible (after save so registration is gone and nav props are clean)
        if (accommodationReservation is not null && accommodationReservation.ResponsibleRegistrationId != command.Id)
        {
            try
            {
                await _emailNotificationService.CreateAndSendEmailAsync(
                    EmailTemplateKey.AccommodationNewResponsible, accommodationReservation.ResponsibleRegistrationId, cancellationToken);
            }
            catch
            {
                // Non-critical: responsibility was already transferred, email is best-effort.
            }
        }

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

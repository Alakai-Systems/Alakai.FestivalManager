namespace Alakai.FestivalManager.Application.Features.Tickets.Services;

public class TicketService : ITicketService
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly ITicketTokenService _ticketTokenService;
    private readonly ITicketPdfService _ticketPdfService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<TicketService> _logger;

    public TicketService(IRegistrationRepository registrationRepository, ITicketTokenService ticketTokenService,
        ITicketPdfService ticketPdfService, IFileStorageService fileStorageService, ILogger<TicketService> logger)
    {
        _registrationRepository = registrationRepository;
        _ticketTokenService = ticketTokenService;
        _ticketPdfService = ticketPdfService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<string?> EnsureTicketGeneratedAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(registrationId, cancellationToken);

        if (registration is null)
        {
            _logger.LogWarning("EnsureTicketGeneratedAsync: registration {RegistrationId} not found.", registrationId);

            return null;
        }

        if (!string.IsNullOrWhiteSpace(registration.TicketPdfUrl))
        {
            return registration.TicketPdfUrl;
        }

        if (registration.PaymentStatus != PaymentStatus.Paid)
        {
            _logger.LogInformation("EnsureTicketGeneratedAsync: registration {RegistrationId} is not fully paid yet ({PaymentStatus}), ticket not generated.",
                registrationId, registration.PaymentStatus);

            return null;
        }

        string token = _ticketTokenService.GenerateToken(registration.Id);
        byte[] qrBytes = _ticketPdfService.GenerateQrCode(token);

        TicketInfo ticketInfo = new()
        {
            ParticipantName = $"{registration.FirstName} {registration.LastName}",
            EventName = registration.Edition.Name,
            PassTypeName = registration.PassType.Name,
            LevelName = registration.Level?.Name,
            Language = registration.Language
        };

        byte[] pdfBytes = _ticketPdfService.GenerateTicketPdf(ticketInfo, qrBytes);

        using (MemoryStream pdfStream = new(pdfBytes))
        {
            registration.TicketPdfUrl = await _fileStorageService.SaveFileAsync(pdfStream, $"ticket-{registration.Id}.pdf", cancellationToken);
        }

        registration.SetUpdated();
        _registrationRepository.Update(registration);
        await _registrationRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket generated for registration {RegistrationId}.", registrationId);

        return registration.TicketPdfUrl;
    }

    public async Task<TicketCheckInResultDto?> CheckInAsync(string token, CancellationToken cancellationToken = default)
    {
        if (!_ticketTokenService.TryValidateToken(token, out Guid registrationId))
        {
            _logger.LogWarning("CheckInAsync: invalid or tampered token.");

            return null;
        }

        Registration? registration = await _registrationRepository.GetByIdAsync(registrationId, cancellationToken);

        if (registration is null)
        {
            _logger.LogWarning("CheckInAsync: registration {RegistrationId} not found.", registrationId);

            return null;
        }

        bool alreadyCheckedIn = registration.CheckedInAt.HasValue;

        if (!alreadyCheckedIn)
        {
            registration.CheckedInAt = DateTime.UtcNow;
            registration.SetUpdated();
            _registrationRepository.Update(registration);
            await _registrationRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Registration {RegistrationId} checked in.", registrationId);
        }

        return new TicketCheckInResultDto
        {
            RegistrationId = registration.Id,
            ParticipantName = $"{registration.FirstName} {registration.LastName}",
            EventName = registration.Edition.Name,
            PassTypeName = registration.PassType.Name,
            LevelName = registration.Level?.Name,
            AlreadyCheckedIn = alreadyCheckedIn,
            CheckedInAt = registration.CheckedInAt!.Value
        };
    }
}
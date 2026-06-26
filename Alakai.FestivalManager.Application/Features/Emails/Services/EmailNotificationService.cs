namespace Alakai.FestivalManager.Application.Features.Emails.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEmailTemplateRendererService _emailTemplateRendererService;
    private readonly IMapper _mapper;

    public EmailNotificationService(IEmailTemplateRepository emailTemplateRepository, IEmailLogRepository emailLogRepository, 
        IEmailTemplateRendererService emailTemplateRendererService, IMapper mapper, IRegistrationRepository registrationRepository)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _emailLogRepository = emailLogRepository;
        _emailTemplateRendererService = emailTemplateRendererService;
        _mapper = mapper;
        _registrationRepository = registrationRepository;
    }

    public async Task<EmailLogDto?> CreateEmailLogAsync(EmailTemplateKey templateKey, Guid registrationId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(registrationId, cancellationToken);

        if (registration is null)
        {
            return null;
        }

        EmailTemplate? template = await _emailTemplateRepository.GetByKeyAsync(templateKey, registration.EditionId, cancellationToken);

        if (template is null)
        {
            return null;
        }

        Dictionary<string, string> variables = new()
        {
            ["FirstName"] = registration.FirstName,
            ["LastName"] = registration.LastName,
            ["FullName"] = $"{registration.FirstName} {registration.LastName}",
            ["Email"] = registration.Email,
            ["EditionName"] = registration.Edition.Name,
            ["EditionYear"] = registration.Edition.Year.ToString(),
            ["PassTypeName"] = registration.PassType.Name,
            ["LevelName"] = registration.Level?.Name ?? string.Empty,
            ["FinalPrice"] = registration.FinalPrice.ToString("0.00"),
            ["PartnerEmail"] = registration.PartnerEmail ?? string.Empty,
            ["RegistrationId"] = registration.Id.ToString(),
            ["FestivalName"] = registration.Edition.Name ?? string.Empty,
            ["PanelUser"] = registration.Email,
            ["PortalUrl"] = "https://lajambarcelona.com/account",
        };

        string subject = _emailTemplateRendererService.Render(template.Subject, variables);
        string bodyHtml = _emailTemplateRendererService.Render(template.BodyHtml, variables);
        string? bodyText = string.IsNullOrWhiteSpace(template.BodyText)
            ? null
            : _emailTemplateRendererService.Render(template.BodyText, variables);

        EmailLog emailLog = new()
        {
            EditionId = registration.EditionId,
            EmailTemplateId = template.Id,
            RegistrationId = registration.Id,
            UserId = registration.UserId,
            TemplateKey = template.TemplateKey,
            RecipientEmail = registration.Email,
            RecipientName = $"{registration.FirstName} {registration.LastName}",
            Subject = subject,
            BodyHtml = bodyHtml,
            BodyText = bodyText,
            Status = EmailLogStatus.Pending,
            IsActive = true
        };

        await _emailLogRepository.AddAsync(emailLog, cancellationToken);
        await _emailLogRepository.SaveChangesAsync(cancellationToken);

        EmailLogDto dto = _mapper.Map<EmailLogDto>(emailLog);

        return dto;
    }
}

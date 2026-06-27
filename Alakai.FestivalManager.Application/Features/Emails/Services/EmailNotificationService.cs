using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Infrastructure.Email;

namespace Alakai.FestivalManager.Application.Features.Emails.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEmailTemplateRendererService _emailTemplateRendererService;
    private readonly IEmailSender _emailSender;
    private readonly IMapper _mapper;

    public EmailNotificationService(IEmailTemplateRepository emailTemplateRepository, IEmailLogRepository emailLogRepository, 
        IEmailTemplateRendererService emailTemplateRendererService, IMapper mapper, IRegistrationRepository registrationRepository, IEmailSender emailSender)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _emailLogRepository = emailLogRepository;
        _emailTemplateRendererService = emailTemplateRendererService;
        _mapper = mapper;
        _registrationRepository = registrationRepository;
        _emailSender = emailSender;
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

    public async Task<EmailLogDto?> CreateAndSendEmailAsync(EmailTemplateKey templateKey, Guid registrationId, CancellationToken cancellationToken = default)
    {
        EmailLogDto? emailLogDto = await CreateEmailLogAsync(templateKey, registrationId, cancellationToken);

        if (emailLogDto is null)
        {
            return null;
        }

        EmailLog? emailLog = await _emailLogRepository.GetByIdAsync(emailLogDto.Id, cancellationToken);

        if (emailLog is null)
        {
            return null;
        }

        try
        {
            EmailMessage message = new()
            {
                To = new EmailAddress
                {
                    Name = emailLog.RecipientName ?? string.Empty,
                    Address = emailLog.RecipientEmail
                },
                Subject = emailLog.Subject,
                HtmlBody = emailLog.BodyHtml,
                TextBody = emailLog.BodyText ?? string.Empty
            };

            await _emailSender.SendAsync(message, cancellationToken);

            emailLog.Status = EmailLogStatus.Sent;
            emailLog.SentAt = DateTime.UtcNow;
            emailLog.ErrorMessage = null;
        }
        catch (Exception ex)
        {
            emailLog.Status = EmailLogStatus.Failed;
            emailLog.ErrorMessage = ex.Message;
        }

        _emailLogRepository.Update(emailLog);
        await _emailLogRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<EmailLogDto>(emailLog);
    }

    public async Task<EmailLogDto?> CreateAndSendPasswordResetEmailAsync(Guid userId, string resetPasswordUrl, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        EmailTemplate? template = await _emailTemplateRepository.GetByKeyAsync(EmailTemplateKey.PasswordReset, null, cancellationToken);

        if (template is null)
        {
            return null;
        }

        Dictionary<string, string> variables = new()
        {
            ["FirstName"] = user.FirstName,
            ["LastName"] = user.LastName,
            ["FullName"] = $"{user.FirstName} {user.LastName}",
            ["Email"] = user.Email,
            ["ResetPasswordUrl"] = resetPasswordUrl
        };

        string subject = _emailTemplateRendererService.Render(template.Subject, variables);
        string bodyHtml = _emailTemplateRendererService.Render(template.BodyHtml, variables);
        string? bodyText = string.IsNullOrWhiteSpace(template.BodyText) ? null : _emailTemplateRendererService.Render(template.BodyText, variables);

        EmailLog emailLog = new()
        {
            UserId = user.Id,
            TemplateKey = template.TemplateKey,
            EmailTemplateId = template.Id,
            RecipientEmail = user.Email,
            RecipientName = $"{user.FirstName} {user.LastName}",
            Subject = subject,
            BodyHtml = bodyHtml,
            BodyText = bodyText,
            Status = EmailLogStatus.Pending,
            IsActive = true
        };

        await _emailLogRepository.AddAsync(emailLog, cancellationToken);
        await _emailLogRepository.SaveChangesAsync(cancellationToken);

        return await SendExistingEmailLogAsync(emailLog.Id, cancellationToken);
    }
}

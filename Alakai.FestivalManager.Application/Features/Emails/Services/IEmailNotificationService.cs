namespace Alakai.FestivalManager.Application.Features.Emails.Services;

public interface IEmailNotificationService
{
    Task<EmailLogDto?> CreateEmailLogAsync(EmailTemplateKey templateKey, Guid registrationId, CancellationToken cancellationToken = default);
}

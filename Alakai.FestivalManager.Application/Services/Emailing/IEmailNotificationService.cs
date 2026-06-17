namespace Alakai.FestivalManager.Application.Services.Emailing;

public interface IEmailNotificationService
{
    Task<EmailLogDto?> CreateEmailLogAsync(EmailTemplateKey templateKey, Guid registrationId, CancellationToken cancellationToken = default);
}

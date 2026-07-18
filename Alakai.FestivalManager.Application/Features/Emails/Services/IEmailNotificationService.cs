namespace Alakai.FestivalManager.Application.Features.Emails.Services;

public interface IEmailNotificationService
{
    Task<EmailLogDto?> CreateEmailLogAsync(EmailTemplateKey templateKey, Guid registrationId, CancellationToken cancellationToken = default);
    Task<EmailLogDto?> CreateAndSendEmailAsync(EmailTemplateKey templateKey, Guid registrationId, CancellationToken cancellationToken = default);
    Task<EmailLogDto?> CreateAndSendPasswordResetEmailAsync(Guid userId, string resetPasswordUrl, CancellationToken cancellationToken = default);
    Task<(string Subject, string Html)?> PreviewTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);
}

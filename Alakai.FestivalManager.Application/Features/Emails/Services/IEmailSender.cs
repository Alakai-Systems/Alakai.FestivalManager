namespace Alakai.FestivalManager.Infrastructure.Email;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, EmailSenderSettings senderSettings, CancellationToken cancellationToken = default);
}
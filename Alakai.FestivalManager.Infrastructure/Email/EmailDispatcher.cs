namespace Alakai.FestivalManager.Infrastructure.Email;

public class EmailDispatcher : IEmailDispatcher
{
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IEmailSender _emailSender;

    public EmailDispatcher(IEmailLogRepository emailLogRepository, IEmailSender emailSender)
    {
        _emailLogRepository = emailLogRepository;
        _emailSender = emailSender;
    }

    public async Task<bool> SendEmailLogAsync(Guid emailLogId, CancellationToken cancellationToken = default)
    {
        EmailLog? emailLog = await _emailLogRepository.GetByIdAsync(emailLogId, cancellationToken);

        if (emailLog is null)
        {
            return false;
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

            await _emailLogRepository.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            emailLog.Status = EmailLogStatus.Failed;
            emailLog.ErrorMessage = ex.Message;

            await _emailLogRepository.SaveChangesAsync(cancellationToken);

            return false;
        }
    }
}
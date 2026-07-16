namespace Alakai.FestivalManager.Infrastructure.Email;

public class MailKitEmailSender : IEmailSender
{
    public async Task SendAsync(EmailMessage message, EmailSenderSettings senderSettings, CancellationToken cancellationToken = default)
    {
        MimeMessage mimeMessage = new();

        mimeMessage.From.Add(new MailboxAddress(senderSettings.FromName, senderSettings.FromEmail));

        mimeMessage.To.Add(new MailboxAddress(message.To.Name, message.To.Address));

        foreach (EmailAddress cc in message.Cc)
        {
            mimeMessage.Cc.Add(new MailboxAddress(cc.Name, cc.Address));
        }

        foreach (EmailAddress bcc in message.Bcc)
        {
            mimeMessage.Bcc.Add(new MailboxAddress(bcc.Name, bcc.Address));
        }

        mimeMessage.Subject = message.Subject;

        BodyBuilder bodyBuilder = new()
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody
        };

        foreach (EmailAttachment attachment in message.Attachments)
        {
            bodyBuilder.Attachments.Add(
                attachment.FileName,
                attachment.Content,
                ContentType.Parse(attachment.ContentType));
        }

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using SmtpClient smtpClient = new();

        SecureSocketOptions socketOptions =
            senderSettings.UseSSL
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

        await smtpClient.ConnectAsync(
            senderSettings.Host,
            senderSettings.Port,
            socketOptions,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(senderSettings.UserName))
        {
            await smtpClient.AuthenticateAsync(
                senderSettings.UserName,
                senderSettings.Password,
                cancellationToken);
        }

        await smtpClient.SendAsync(mimeMessage, cancellationToken);

        await smtpClient.DisconnectAsync(true, cancellationToken);
    }
}
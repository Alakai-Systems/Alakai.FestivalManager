namespace Alakai.FestivalManager.Infrastructure.Email;

public class MailKitEmailSender : IEmailSender
{
    private static readonly TimeSpan SmtpOperationTimeout = TimeSpan.FromSeconds(20);

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

        // Puerto 465 = SSL/TLS implicito desde el primer byte (SslOnConnect).
        // Puerto 587 (u otros) = conectar en plano y subir a TLS (StartTls).
        // Usar StartTls contra un servidor en el puerto 465 hace que muchos
        // servidores no respondan nada, en vez de rechazar limpiamente.
        SecureSocketOptions socketOptions = senderSettings switch
        {
            { UseSSL: false } => SecureSocketOptions.None,
            { Port: 465 } => SecureSocketOptions.SslOnConnect,
            _ => SecureSocketOptions.StartTls
        };

        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(SmtpOperationTimeout);

        try
        {
            await smtpClient.ConnectAsync(
                senderSettings.Host,
                senderSettings.Port,
                socketOptions,
                timeoutCts.Token);

            if (!string.IsNullOrWhiteSpace(senderSettings.UserName))
            {
                await smtpClient.AuthenticateAsync(
                    senderSettings.UserName,
                    senderSettings.Password,
                    timeoutCts.Token);
            }

            await smtpClient.SendAsync(mimeMessage, timeoutCts.Token);

            await smtpClient.DisconnectAsync(true, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"Could not complete the SMTP operation with '{senderSettings.Host}:{senderSettings.Port}' within {SmtpOperationTimeout.TotalSeconds} seconds. Check the email server settings for this festival.");
        }
    }
}
namespace Alakai.FestivalManager.Infrastructure.Email;

public class MailKitEmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public MailKitEmailSender(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        MimeMessage mimeMessage = new();

        mimeMessage.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));

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
            _options.UseSSL
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

        await smtpClient.ConnectAsync(
            _options.Host,
            _options.Port,
            socketOptions,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            await smtpClient.AuthenticateAsync(
                _options.UserName,
                _options.Password,
                cancellationToken);
        }

        await smtpClient.SendAsync(mimeMessage, cancellationToken);

        await smtpClient.DisconnectAsync(true, cancellationToken);
    }
}
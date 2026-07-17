# Fix-Step20-SmtpProtocolByPort.ps1
#
# CAUSA RAIZ REAL (no solo un timeout): el codigo siempre usaba
# SecureSocketOptions.StartTls para cifrar, sea cual sea el puerto. Pero:
#   - Puerto 587 -> STARTTLS (conectar en plano, luego subir a TLS) - correcto.
#   - Puerto 465 -> SSL/TLS IMPLICITO desde el primer byte - StartTls es el
#     protocolo EQUIVOCADO para este puerto.
#
# Las credenciales SMTP tanto de La Jam como de Swim Out usan el puerto 465,
# asi que estabamos negociando STARTTLS contra un servidor que espera TLS
# implicito desde el primer byte. Muchos servidores, ante esa negociacion
# incorrecta, no la rechazan limpiamente - simplemente no responden nada,
# lo que produce exactamente el "se queda colgado" que estabas viendo.
#
# Fix: elegir el modo correcto segun el puerto (465 -> SslOnConnect,
# 587 -> StartTls), manteniendo tambien un timeout de seguridad de 20s por si
# el servidor de verdad no responde por otro motivo (red, firewall, etc.).
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Infrastructure/Email/MailKitEmailSender.cs"

if (-not (Test-Path $path)) {
    Write-Host "SKIP (archivo no encontrado): $path" -ForegroundColor Yellow
    exit 1
}

$content = @'
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
'@

Set-Content -Path $path -Value $content -NoNewline
Write-Host "OK: MailKitEmailSender.cs corregido (SslOnConnect para puerto 465)." -ForegroundColor Green
Write-Host "dotnet build para confirmar." -ForegroundColor Yellow
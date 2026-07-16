namespace Alakai.FestivalManager.Infrastructure.Email;

/// <summary>
/// Cuenta de email de "sistema", usada UNICAMENTE para el envio de reset de
/// contrasena (User es global, no tiene festival asociado). Vive en Application
/// (igual que EmailSenderSettings/IEmailSender) para no crear una dependencia
/// de Application hacia Infrastructure.
/// </summary>
public class SystemEmailOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSSL { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
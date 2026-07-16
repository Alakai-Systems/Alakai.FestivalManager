namespace Alakai.FestivalManager.Infrastructure.Email;

/// <summary>
/// Credenciales SMTP concretas para un envio dado. Se construyen a partir de
/// FestivalCredentials (envios ligados a una Registration) o de SystemEmailOptions
/// (la cuenta de sistema, solo para el email de reset de contrasena).
/// NOTA: vive fisicamente en Application (igual que IEmailSender) aunque el
/// namespace diga "Infrastructure.Email" - es la convencion ya existente en este
/// proyecto para esta interfaz/modelos relacionados.
/// </summary>
public class EmailSenderSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSSL { get; set; } = true;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
namespace Alakai.FestivalManager.Infrastructure.Email;

/// <summary>
/// Fallback global para el enlace al panel de usuario en los emails, usado
/// SOLO cuando el Festival de la registration no tiene su propio CustomDomain
/// configurado. Se enlaza desde la seccion "ApplicationUrls" de appsettings.
/// </summary>
public class ApplicationUrlsOptions
{
    public string PortalUrl { get; set; } = string.Empty;
}
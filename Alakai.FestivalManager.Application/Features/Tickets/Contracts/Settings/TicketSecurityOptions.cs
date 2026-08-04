namespace Alakai.FestivalManager.Application.Features.Tickets.Contracts.Settings;

public class TicketSecurityOptions
{
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>URL publica base de la Api (ej. https://app-alakai-swimout-api.azurewebsites.net),
    /// usada para construir la URL de check-in que se codifica en el QR de la entrada.</summary>
    public string PublicApiBaseUrl { get; set; } = string.Empty;
}
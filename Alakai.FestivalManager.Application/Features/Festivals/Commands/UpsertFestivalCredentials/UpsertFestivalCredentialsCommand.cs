namespace Alakai.FestivalManager.Application.Features.Festivals.Commands.UpsertFestivalCredentials;

public class UpsertFestivalCredentialsCommand
{
    public Guid FestivalId { get; set; }
    public string RedsysMerchantCode { get; set; } = string.Empty;
    public string RedsysTerminal { get; set; } = string.Empty;

    /// <summary>Vacio = conservar la clave ya guardada (no se sobreescribe).</summary>
    public string? RedsysSecretKey { get; set; }
    public string RedsysMerchantName { get; set; } = string.Empty;
    public string EmailHost { get; set; } = string.Empty;
    public int EmailPort { get; set; }
    public string EmailUsername { get; set; } = string.Empty;

    /// <summary>Vacio = conservar la contrasena ya guardada (no se sobreescribe).</summary>
    public string? EmailPassword { get; set; }
    public string EmailFromEmail { get; set; } = string.Empty;
    public string EmailFromName { get; set; } = string.Empty;
    public bool EmailUseSSL { get; set; } = true;
}
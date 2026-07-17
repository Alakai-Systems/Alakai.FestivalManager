namespace Alakai.FestivalManager.Admin.Contracts.Festivals.Requests;

public class UpsertFestivalCredentialsRequest
{
    public string RedsysMerchantCode { get; set; } = string.Empty;
    public string RedsysTerminal { get; set; } = string.Empty;

    /// <summary>Vacio = conservar la clave ya guardada.</summary>
    public string? RedsysSecretKey { get; set; }
    public string RedsysMerchantName { get; set; } = string.Empty;
    public string? RedsysPaymentUrl { get; set; }
    public string EmailHost { get; set; } = string.Empty;
    public int EmailPort { get; set; } = 587;
    public string EmailUsername { get; set; } = string.Empty;

    /// <summary>Vacio = conservar la contrasena ya guardada.</summary>
    public string? EmailPassword { get; set; }
    public string EmailFromEmail { get; set; } = string.Empty;
    public string EmailFromName { get; set; } = string.Empty;
    public bool EmailUseSSL { get; set; } = true;
}
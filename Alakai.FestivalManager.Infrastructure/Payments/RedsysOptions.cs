namespace Alakai.FestivalManager.Infrastructure.Payments;

/// <summary>
/// Configuracion de Redsys COMUN a toda la app (no depende del festival).
/// MerchantCode/Terminal/SecretKey/MerchantName viven ahora en FestivalCredentials,
/// porque cada festival cobra en su propia cuenta.
/// </summary>
public class RedsysOptions
{
    public string Currency { get; set; } = "978";
    public string PaymentUrl { get; set; } = "https://sis-t.redsys.es:25443/sis/realizarPago";
    public string NotificationUrl { get; set; } = string.Empty;
    public string UrlOk { get; set; } = string.Empty;
    public string UrlKo { get; set; } = string.Empty;
}
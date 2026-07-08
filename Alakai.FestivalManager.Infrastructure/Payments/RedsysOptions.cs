namespace Alakai.FestivalManager.Infrastructure.Payments;

public class RedsysOptions
{
    public string MerchantCode { get; set; } = string.Empty;
    public string Terminal { get; set; } = string.Empty;
    public string Currency { get; set; } = "978";
    public string SecretKey { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = "https://sis-t.redsys.es:25443/sis/realizarPago";
    public string NotificationUrl { get; set; } = string.Empty;
    public string UrlOk { get; set; } = string.Empty;
    public string UrlKo { get; set; } = string.Empty;
}

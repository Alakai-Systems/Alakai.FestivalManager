namespace Alakai.FestivalManager.Admin.Contracts.Payments.Requests;

public class CreatePaymentSessionRequest
{
    public Guid RegistrationId { get; set; }
    public string? UrlOk { get; set; }
    public string? UrlKo { get; set; }
    public decimal? AmountOverride { get; set; }
}

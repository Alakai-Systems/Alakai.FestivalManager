namespace Alakai.FestivalManager.Admin.Contracts.Payments.Requests;

public class ConfirmRedsysReturnRequest
{
    public string MerchantParameters { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

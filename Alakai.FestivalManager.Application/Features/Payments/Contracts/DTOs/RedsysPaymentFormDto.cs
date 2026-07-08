namespace Alakai.FestivalManager.Application.Features.Payments.Contracts.DTOs;

public class RedsysPaymentFormDto
{
    public string Url { get; set; } = string.Empty;
    public string SignatureVersion { get; set; } = string.Empty;
    public string MerchantParameters { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string Order { get; set; } = string.Empty;
}

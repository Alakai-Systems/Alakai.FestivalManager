namespace Alakai.FestivalManager.Application.Features.Payments.Commands.ConfirmRedsysReturn;

public class ConfirmRedsysReturnCommand
{
    public string MerchantParameters { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

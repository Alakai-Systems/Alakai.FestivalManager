namespace Alakai.FestivalManager.Application.Features.Payments.Commands.CreatePaymentSession;

public class CreatePaymentSessionCommand
{
    public Guid RegistrationId { get; set; }
    public string? UrlOk { get; set; }
    public string? UrlKo { get; set; }
    /// <summary>When set, overrides FinalPrice from the registration (e.g. 50% split).</summary>
    public decimal? AmountOverride { get; set; }
}

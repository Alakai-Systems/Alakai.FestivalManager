namespace Alakai.FestivalManager.Admin.Contracts.Payments.DTOs;

public class StripeCheckoutSessionDto
{
    public string Url { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}
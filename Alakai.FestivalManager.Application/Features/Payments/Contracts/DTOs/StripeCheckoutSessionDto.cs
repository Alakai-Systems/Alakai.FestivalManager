namespace Alakai.FestivalManager.Application.Features.Payments.Contracts.DTOs;

public class StripeCheckoutSessionDto
{
    public string Url { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}
namespace Alakai.FestivalManager.Application.Features.Payments.Contracts.DTOs;

public class StripeNotificationDto
{
    public string Order { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public long AmountInCents { get; set; }
    public string? PaymentIntentId { get; set; }
}
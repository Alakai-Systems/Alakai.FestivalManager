namespace Alakai.FestivalManager.Application.Features.Payments.Contracts.DTOs;

public class RedsysNotificationDto
{
    public string Order { get; set; } = string.Empty;
    public int ResponseCode { get; set; } = -1;
    public long AmountInCents { get; set; }
    public string? AuthorisationCode { get; set; }
    public bool IsApproved => ResponseCode >= 0 && ResponseCode <= 99;
}

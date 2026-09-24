namespace Alakai.FestivalManager.Application.Features.Payments.Contracts.DTOs;

public class RedsysRefundResultDto
{
    public bool Success { get; set; }
    public string? ResponseCode { get; set; }
    public string Message { get; set; } = string.Empty;
}
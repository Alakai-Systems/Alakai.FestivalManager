namespace Alakai.FestivalManager.Application.Features.Payments.Commands.RefundRegistration;

public class RefundRegistrationCommand
{
    public Guid RegistrationId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}
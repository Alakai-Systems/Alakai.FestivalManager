namespace Alakai.FestivalManager.Admin.Contracts.Payments.Requests;

public class RefundRegistrationRequest
{
    public Guid RegistrationId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}
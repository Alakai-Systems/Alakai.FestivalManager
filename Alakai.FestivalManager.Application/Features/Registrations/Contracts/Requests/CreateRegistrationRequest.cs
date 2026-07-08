namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Requests;

public class CreateRegistrationRequest
{
    public Guid EditionId { get; set; }
    public Guid PassTypeId { get; set; }
    public Guid? LevelId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string Password { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string? DocumentCountry { get; set; }
    public DanceRole? DanceRole { get; set; }
    public string? PartnerEmail { get; set; }
    public decimal BasePrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public string? DiscountCodeValue { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public List<Guid> LevelIds { get; set; } = [];
    public PaymentPlan PaymentPlan { get; set; } = PaymentPlan.FullOnline;
    public decimal ManagementFee { get; set; }
    public decimal AmountPaid { get; set; }
}

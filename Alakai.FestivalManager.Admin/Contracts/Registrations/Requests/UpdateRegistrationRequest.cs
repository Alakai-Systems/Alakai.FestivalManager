namespace Alakai.FestivalManager.Admin.Contracts.Registrations.Requests;

public class UpdateRegistrationRequest
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public Guid PassTypeId { get; set; }
    public Guid? LevelId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? DocumentNumber { get; set; }
    public string? DocumentCountry { get; set; }
    public DanceRole? DanceRole { get; set; }
    public string? PartnerEmail { get; set; }
    public decimal BasePrice { get; set; }
    public string? DiscountCodeValue { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public RegistrationStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public bool IsActive { get; set; }
}

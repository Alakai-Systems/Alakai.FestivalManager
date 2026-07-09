namespace Alakai.FestivalManager.Admin.Contracts.Registrations.DTOs;

public class RegistrationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid EditionId { get; set; }
    public Guid PassTypeId { get; set; }
    public Guid? LevelId { get; set; }
    public Guid? DiscountCodeId { get; set; }
    public string? DiscountCode { get; set; }
    public string? DiscountCodeValue { get; set; }
    public DiscountApplicationStatus DiscountStatus { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? DocumentNumber { get; set; }
    public string? DocumentCountry { get; set; }
    public DanceRole? DanceRole { get; set; }
    public string? PartnerEmail { get; set; }
    public string? PartnerName { get; set; }
    public Guid? PartnerRegistrationId { get; set; }
    public RegistrationStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal BasePrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public string? PaymentReference { get; set; }
    public string? PaymentAuthCodes { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool IsActive { get; set; }
    public string GroupCode => !string.IsNullOrWhiteSpace(DiscountCodeValue) ? DiscountCodeValue! : DiscountCode ?? string.Empty;
    public PaymentPlan PaymentPlan { get; set; }
    public decimal ManagementFee { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? PaymentDueAt { get; set; }
}

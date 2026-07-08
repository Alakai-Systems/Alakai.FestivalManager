namespace Alakai.FestivalManager.Domain.Entities;

public class Registration : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public Guid PassTypeId { get; set; }
    public PassType PassType { get; set; } = default!;

    public Guid? LevelId { get; set; }
    public Level? Level { get; set; }
    public ICollection<RegistrationLevelSelection> LevelSelections { get; set; } = [];

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
    public Guid? PartnerRegistrationId { get; set; }
    public Registration? PartnerRegistration { get; set; }

    public RegistrationStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public Guid? DiscountCodeId { get; set; }
    public DiscountCode? DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public DiscountApplicationStatus DiscountStatus { get; set; }

    public string? DiscountCodeValue { get; set; }

    public decimal BasePrice { get; set; }
    public decimal FinalPrice { get; set; }

    public string? PaymentReference { get; set; }
    public DateTime? PaidAt { get; set; }

    public PaymentPlan PaymentPlan { get; set; } = PaymentPlan.FullOnline;
    public decimal ManagementFee { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? PaymentDueAt { get; set; }

    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    public DateTime? CancelledAt { get; set; }

    public bool IsActive { get; set; } = true;
}

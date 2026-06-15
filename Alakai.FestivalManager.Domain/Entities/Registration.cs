using System;
using Alakai.FestivalManager.Domain.Enums;

namespace Alakai.FestivalManager.Domain.Entities;

public class Registration : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public Guid PassTypeId { get; set; }
    public PassType PassType { get; set; } = default!;

    public Guid? LevelId { get; set; }
    public Level? Level { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }

    public DanceRole? DanceRole { get; set; }
    public string? PartnerEmail { get; set; }
    public Guid? PartnerRegistrationId { get; set; }
    public Registration? PartnerRegistration { get; set; }

    public RegistrationStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public decimal BasePrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public string? DiscountCode { get; set; }

    public string? PaymentReference { get; set; }
    public DateTime? PaidAt { get; set; }

    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public bool IsActive { get; set; } = true;
}

namespace Alakai.FestivalManager.Application.Features.UserPanel.Contracts.DTOs;

public class UserPanelRegistrationDto
{
    public Guid Id { get; set; }
    public string? EditionName { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PassTypeName { get; set; } = string.Empty;
    public string? LevelName { get; set; }
    public string? DanceRole { get; set; }
    public string? PartnerEmail { get; set; }
    public string? DiscountCodeValue { get; set; }
    public decimal FinalPrice { get; set; }
    public string? DocumentNumber { get; set; }
    public string? DocumentCountry { get; set; }
    public string PaymentPlan { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public DateTime? PaymentDueAt { get; set; }
    public string Language { get; set; } = "en";
}
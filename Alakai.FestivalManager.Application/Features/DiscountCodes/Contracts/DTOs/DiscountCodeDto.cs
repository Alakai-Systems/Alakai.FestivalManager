namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.DTOs;

public class DiscountCodeDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DiscountActivationType ActivationType { get; set; }
    public int? ActivationThreshold { get; set; }
    public int? MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; }
}

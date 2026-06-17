namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Contracts.DTOs;

public class DiscountCalculationResult
{
    public Guid? DiscountCodeId { get; set; }
    public string? DiscountCodeValue { get; set; }

    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }

    public DiscountApplicationStatus DiscountStatus { get; set; }
}

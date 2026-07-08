namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;

public class PublicDiscountCheckDto
{
    public bool Exists { get; set; }
    public bool Applied { get; set; }
    public bool PendingThreshold { get; set; }

    /// <summary>Actual currency amount discounted (works for both fixed and percentage codes).</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Final price after the discount has been applied to the supplied base price.</summary>
    public decimal FinalPrice { get; set; }
}

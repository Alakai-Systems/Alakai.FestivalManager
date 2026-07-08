namespace Alakai.FestivalManager.Admin.Contracts.Registrations.DTOs;

public class PublicDiscountCheckDto
{
    public bool Exists { get; set; }
    public bool Applied { get; set; }
    public bool PendingThreshold { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
}

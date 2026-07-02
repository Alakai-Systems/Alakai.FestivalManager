namespace Alakai.FestivalManager.Admin.Contracts.UserPanel.DTOs;

public class UserPanelInvoiceDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string PdfUrl { get; set; } = string.Empty;
}
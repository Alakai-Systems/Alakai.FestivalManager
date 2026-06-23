namespace Alakai.FestivalManager.Application.Features.UserPanel.Contracts.DTOs;

public class UserPanelInvoiceDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}
namespace Alakai.FestivalManager.Admin.Contracts.UserPanel.Requests;

public class CreateUserPanelInvoiceRequest
{
    public string FiscalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
namespace Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoice;

public class UpdateInvoiceCommand
{
    public Guid Id { get; set; }
    public string FiscalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
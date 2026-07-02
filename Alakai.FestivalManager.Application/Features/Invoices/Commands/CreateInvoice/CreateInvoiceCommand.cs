namespace Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommand
{
    public Guid RegistrationId { get; set; }
    public string FiscalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
namespace Alakai.FestivalManager.Application.Features.Invoices.Contracts.DTOs;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid RegistrationId { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public decimal Amount { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public string PdfUrl { get; set; } = string.Empty;
    public string FiscalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? CustomerFirstName { get; set; }
    public string? CustomerLastName { get; set; }
    public string? CustomerEmail { get; set; }
    public Guid? EditionId { get; set; }
    public string? EditionName { get; set; }
}
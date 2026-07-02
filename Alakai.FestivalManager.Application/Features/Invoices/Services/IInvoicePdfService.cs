using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Invoices.Services;

public class InvoiceIssuerInfo
{
    public string CompanyName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public byte[]? LogoBytes { get; set; }
}

public interface IInvoicePdfService
{
    byte[] GenerateInvoicePdf(Invoice invoice, string eventName, string passTypeName, string participantName, InvoiceIssuerInfo issuer);
}
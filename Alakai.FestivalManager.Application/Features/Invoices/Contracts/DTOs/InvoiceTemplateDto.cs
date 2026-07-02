namespace Alakai.FestivalManager.Application.Features.Invoices.Contracts.DTOs;

public class InvoiceTemplateDto
{
    public Guid Id { get; set; }
    public Guid? EditionId { get; set; }
    public string? EditionName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
}
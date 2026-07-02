namespace Alakai.FestivalManager.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid RegistrationId { get; set; }
    public Registration Registration { get; set; } = default!;

    public string Number { get; set; } = string.Empty;
    public int Year { get; set; }
    public int SequenceNumber { get; set; }

    public DateTime IssuedAt { get; set; }
    public decimal Amount { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }

    public string FiscalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public string PdfUrl { get; set; } = string.Empty;
}
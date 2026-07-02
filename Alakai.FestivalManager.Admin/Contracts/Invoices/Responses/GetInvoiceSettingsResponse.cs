using Alakai.FestivalManager.Admin.Contracts.Invoices.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Invoices.Responses;

public class GetInvoiceSettingsResponse
{
    public InvoiceSettingsDto Settings { get; set; } = new();
}
using Alakai.FestivalManager.Admin.Contracts.Invoices.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Invoices.Responses;

public class UpdateInvoiceSettingsResponse
{
    public InvoiceSettingsDto Settings { get; set; } = new();
}
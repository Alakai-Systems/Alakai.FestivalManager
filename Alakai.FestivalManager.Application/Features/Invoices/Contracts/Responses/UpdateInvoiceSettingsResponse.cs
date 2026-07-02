using Alakai.FestivalManager.Application.Features.Invoices.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses;

public class UpdateInvoiceSettingsResponse
{
    public InvoiceSettingsDto Settings { get; set; } = new();
}

namespace Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses;

public class GetInvoiceSettingsResponse
{
    public InvoiceSettingsDto Settings { get; set; } = new();
}
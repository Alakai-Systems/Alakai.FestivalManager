using Alakai.FestivalManager.Admin.Contracts.Invoices.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Invoices.Responses;

public class UpdateInvoiceTemplateResponse
{
    public InvoiceTemplateDto Template { get; set; } = default!;
}
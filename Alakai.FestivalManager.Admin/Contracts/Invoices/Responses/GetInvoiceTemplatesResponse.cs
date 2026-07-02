using Alakai.FestivalManager.Admin.Contracts.Invoices.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Invoices.Responses;

public class GetInvoiceTemplatesResponse
{
    public IReadOnlyList<InvoiceTemplateDto> Templates { get; set; } = [];
}
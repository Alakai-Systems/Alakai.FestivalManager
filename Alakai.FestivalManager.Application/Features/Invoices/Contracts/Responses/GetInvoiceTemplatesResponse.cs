using Alakai.FestivalManager.Application.Features.Invoices.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses;

public class GetInvoiceTemplatesResponse
{
    public IReadOnlyList<InvoiceTemplateDto> Templates { get; set; } = [];
}
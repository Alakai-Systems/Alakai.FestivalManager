using Alakai.FestivalManager.Application.Features.Invoices.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses;

public class UpdateInvoiceTemplateResponse
{
    public InvoiceTemplateDto Template { get; set; } = default!;
}
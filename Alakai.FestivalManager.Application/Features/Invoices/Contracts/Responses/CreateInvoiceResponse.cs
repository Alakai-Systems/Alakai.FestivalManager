using Alakai.FestivalManager.Application.Features.Invoices.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses;

public class CreateInvoiceResponse
{
    public InvoiceDto Invoice { get; set; } = default!;
}
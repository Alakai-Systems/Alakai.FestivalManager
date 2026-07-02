using Alakai.FestivalManager.Application.Features.Invoices.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses;

public class GetInvoicesResponse
{
    public IReadOnlyList<InvoiceDto> Invoices { get; set; } = [];
}
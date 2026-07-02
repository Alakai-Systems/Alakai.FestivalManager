using Alakai.FestivalManager.Admin.Contracts.Invoices.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Invoices.Responses;

public class GetInvoicesResponse
{
    public IReadOnlyList<InvoiceDto> Invoices { get; set; } = [];
}
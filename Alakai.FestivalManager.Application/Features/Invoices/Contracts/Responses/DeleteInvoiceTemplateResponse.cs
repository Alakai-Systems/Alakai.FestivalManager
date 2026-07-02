namespace Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses;

public class DeleteInvoiceTemplateResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoice;

namespace Alakai.FestivalManager.Application.Features.Invoices.Services;

public interface IInvoiceService
{
    Task<ApiResponse<CreateInvoiceResponse>> CreateAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetInvoicesResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses.UpdateInvoiceResponse>> UpdateAsync(Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoice.UpdateInvoiceCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses.DeleteInvoiceResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
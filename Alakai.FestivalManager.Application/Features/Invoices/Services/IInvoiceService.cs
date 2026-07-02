using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoice;
using Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses;

namespace Alakai.FestivalManager.Application.Features.Invoices.Services;

public interface IInvoiceService
{
    Task<ApiResponse<CreateInvoiceResponse>> CreateAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetInvoicesResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}
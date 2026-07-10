using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoiceTemplate;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoiceTemplate;

namespace Alakai.FestivalManager.Application.Features.Invoices.Services;

public interface IInvoiceTemplateService
{
    Task<ApiResponse<GetInvoiceTemplatesResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateInvoiceTemplateResponse>> CreateAsync(CreateInvoiceTemplateCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateInvoiceTemplateResponse>> UpdateAsync(UpdateInvoiceTemplateCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteInvoiceTemplateResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
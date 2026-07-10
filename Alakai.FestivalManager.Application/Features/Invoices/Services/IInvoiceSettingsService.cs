using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoiceSettings;

namespace Alakai.FestivalManager.Application.Features.Invoices.Services;

public interface IInvoiceSettingsService
{
    Task<ApiResponse<GetInvoiceSettingsResponse>> GetAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateInvoiceSettingsResponse>> UpdateAsync(UpdateInvoiceSettingsCommand command, CancellationToken cancellationToken = default);
}
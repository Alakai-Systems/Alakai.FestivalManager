using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoiceSettings;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Invoices.Services;

public class InvoiceSettingsService : IInvoiceSettingsService
{
    private readonly IInvoiceSettingsRepository _invoiceSettingsRepository;
    private readonly UpdateInvoiceSettingsHandler _updateInvoiceSettingsHandler;

    public InvoiceSettingsService(IInvoiceSettingsRepository invoiceSettingsRepository, UpdateInvoiceSettingsHandler updateInvoiceSettingsHandler)
    {
        _invoiceSettingsRepository = invoiceSettingsRepository;
        _updateInvoiceSettingsHandler = updateInvoiceSettingsHandler;
    }

    public async Task<ApiResponse<GetInvoiceSettingsResponse>> GetAsync(CancellationToken cancellationToken = default)
    {
        InvoiceSettings? settings = await _invoiceSettingsRepository.GetAsync(cancellationToken);

        InvoiceSettingsDto dto = settings is null
            ? new InvoiceSettingsDto()
            : new InvoiceSettingsDto
            {
                CompanyName = settings.CompanyName,
                TaxId = settings.TaxId,
                Address = settings.Address,
                City = settings.City,
                PostalCode = settings.PostalCode,
                Country = settings.Country,
                LogoUrl = settings.LogoUrl
            };

        return new ApiResponse<GetInvoiceSettingsResponse> { Success = true, Data = new GetInvoiceSettingsResponse { Settings = dto }, Errors = [], Message = "Invoice settings loaded." };
    }

    public async Task<ApiResponse<UpdateInvoiceSettingsResponse>> UpdateAsync(UpdateInvoiceSettingsCommand command, CancellationToken cancellationToken = default)
    {
        InvoiceSettingsDto dto = await _updateInvoiceSettingsHandler.HandleAsync(command, cancellationToken);
        return new ApiResponse<UpdateInvoiceSettingsResponse> { Success = true, Data = new UpdateInvoiceSettingsResponse { Settings = dto }, Errors = [], Message = "Invoice settings updated." };
    }
}
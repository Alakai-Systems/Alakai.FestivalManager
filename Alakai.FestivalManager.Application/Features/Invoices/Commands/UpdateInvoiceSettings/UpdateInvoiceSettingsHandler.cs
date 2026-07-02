using Alakai.FestivalManager.Application.Features.Invoices.Contracts.DTOs;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoiceSettings;

public class UpdateInvoiceSettingsHandler
{
    private readonly IInvoiceSettingsRepository _invoiceSettingsRepository;

    public UpdateInvoiceSettingsHandler(IInvoiceSettingsRepository invoiceSettingsRepository)
    {
        _invoiceSettingsRepository = invoiceSettingsRepository;
    }

    public async Task<InvoiceSettingsDto> HandleAsync(UpdateInvoiceSettingsCommand command, CancellationToken cancellationToken = default)
    {
        InvoiceSettings? settings = await _invoiceSettingsRepository.GetAsync(cancellationToken);

        if (settings is null)
        {
            settings = new InvoiceSettings();
            await _invoiceSettingsRepository.AddAsync(settings, cancellationToken);
        }

        settings.CompanyName = command.CompanyName;
        settings.TaxId = command.TaxId;
        settings.Address = command.Address;
        settings.City = command.City;
        settings.PostalCode = command.PostalCode;
        settings.Country = command.Country;
        settings.LogoUrl = command.LogoUrl;

        await _invoiceSettingsRepository.SaveChangesAsync(cancellationToken);

        return new InvoiceSettingsDto
        {
            CompanyName = settings.CompanyName,
            TaxId = settings.TaxId,
            Address = settings.Address,
            City = settings.City,
            PostalCode = settings.PostalCode,
            Country = settings.Country,
            LogoUrl = settings.LogoUrl
        };
    }
}
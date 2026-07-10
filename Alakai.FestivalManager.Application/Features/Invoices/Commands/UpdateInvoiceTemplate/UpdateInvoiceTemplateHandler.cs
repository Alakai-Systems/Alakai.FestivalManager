using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoiceTemplate;

public class UpdateInvoiceTemplateHandler
{
    private readonly IInvoiceTemplateRepository _invoiceTemplateRepository;

    public UpdateInvoiceTemplateHandler(IInvoiceTemplateRepository invoiceTemplateRepository)
    {
        _invoiceTemplateRepository = invoiceTemplateRepository;
    }

    public async Task<InvoiceTemplateDto> HandleAsync(UpdateInvoiceTemplateCommand command, CancellationToken cancellationToken = default)
    {
        InvoiceTemplate? template = await _invoiceTemplateRepository.GetByIdAsync(command.Id, cancellationToken);

        if (template is null)
        {
            throw new NotFoundException($"Invoice template with id '{command.Id}' was not found.");
        }

        template.EditionId = command.EditionId;
        template.Name = command.Name;
        template.CompanyName = command.CompanyName;
        template.TaxId = command.TaxId;
        template.Address = command.Address;
        template.City = command.City;
        template.PostalCode = command.PostalCode;
        template.Country = command.Country;
        template.LogoUrl = command.LogoUrl;
        template.IsActive = command.IsActive;

        await _invoiceTemplateRepository.SaveChangesAsync(cancellationToken);

        InvoiceTemplate? refreshed = await _invoiceTemplateRepository.GetByIdAsync(template.Id, cancellationToken);

        return new InvoiceTemplateDto
        {
            Id = template.Id,
            EditionId = template.EditionId,
            EditionName = refreshed?.Edition?.Name,
            Name = template.Name,
            CompanyName = template.CompanyName,
            TaxId = template.TaxId,
            Address = template.Address,
            City = template.City,
            PostalCode = template.PostalCode,
            Country = template.Country,
            LogoUrl = template.LogoUrl,
            IsActive = template.IsActive
        };
    }
}
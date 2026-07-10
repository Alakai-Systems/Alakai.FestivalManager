using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoiceTemplate;

public class CreateInvoiceTemplateHandler
{
    private readonly IInvoiceTemplateRepository _invoiceTemplateRepository;

    public CreateInvoiceTemplateHandler(IInvoiceTemplateRepository invoiceTemplateRepository)
    {
        _invoiceTemplateRepository = invoiceTemplateRepository;
    }

    public async Task<InvoiceTemplateDto> HandleAsync(CreateInvoiceTemplateCommand command, CancellationToken cancellationToken = default)
    {
        InvoiceTemplate template = new()
        {
            EditionId = command.EditionId,
            Name = command.Name,
            CompanyName = command.CompanyName,
            TaxId = command.TaxId,
            Address = command.Address,
            City = command.City,
            PostalCode = command.PostalCode,
            Country = command.Country,
            LogoUrl = command.LogoUrl,
            IsActive = command.IsActive
        };

        await _invoiceTemplateRepository.AddAsync(template, cancellationToken);
        await _invoiceTemplateRepository.SaveChangesAsync(cancellationToken);

        InvoiceTemplate? saved = await _invoiceTemplateRepository.GetByIdAsync(template.Id, cancellationToken);

        return new InvoiceTemplateDto
        {
            Id = template.Id,
            EditionId = template.EditionId,
            EditionName = saved?.Edition?.Name,
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
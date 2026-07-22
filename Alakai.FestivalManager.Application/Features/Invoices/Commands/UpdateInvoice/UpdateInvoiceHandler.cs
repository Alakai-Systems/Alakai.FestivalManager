using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Files.Services;
using Alakai.FestivalManager.Application.Features.Invoices.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.Invoices.Services;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoice;

public class UpdateInvoiceHandler
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoiceTemplateRepository _invoiceTemplateRepository;
    private readonly IInvoicePdfService _invoicePdfService;
    private readonly IFileStorageService _fileStorageService;

    public UpdateInvoiceHandler(IInvoiceRepository invoiceRepository, IInvoiceTemplateRepository invoiceTemplateRepository, IInvoicePdfService invoicePdfService, IFileStorageService fileStorageService)
    {
        _invoiceRepository = invoiceRepository;
        _invoiceTemplateRepository = invoiceTemplateRepository;
        _invoicePdfService = invoicePdfService;
        _fileStorageService = fileStorageService;
    }

    public async Task<InvoiceDto> HandleAsync(UpdateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        Invoice? invoice = await _invoiceRepository.GetByIdAsync(command.Id, cancellationToken);

        if (invoice is null)
        {
            throw new NotFoundException($"Invoice with id '{command.Id}' was not found.");
        }

        invoice.FiscalName = command.FiscalName;
        invoice.TaxId = command.TaxId;
        invoice.Address = command.Address;
        invoice.City = command.City;
        invoice.PostalCode = command.PostalCode;
        invoice.Country = command.Country;

        InvoiceTemplate? template = await _invoiceTemplateRepository.GetForEditionAsync(invoice.Registration.EditionId, cancellationToken);

        byte[]? logoBytes = null;

        if (template is not null && !string.IsNullOrWhiteSpace(template.LogoUrl))
        {
            string? localPath = _fileStorageService.ResolveLocalPath(template.LogoUrl);

            if (localPath is not null && File.Exists(localPath))
            {
                logoBytes = await File.ReadAllBytesAsync(localPath, cancellationToken);
            }
        }

        InvoiceIssuerInfo issuer = new()
        {
            CompanyName = template?.CompanyName ?? string.Empty,
            TaxId = template?.TaxId ?? string.Empty,
            Address = template?.Address ?? string.Empty,
            City = template?.City ?? string.Empty,
            PostalCode = template?.PostalCode ?? string.Empty,
            Country = template?.Country ?? string.Empty,
            LogoBytes = logoBytes
        };

        string participantName = $"{invoice.Registration.FirstName} {invoice.Registration.LastName}";
        byte[] pdfBytes = _invoicePdfService.GenerateInvoicePdf(invoice, invoice.Registration.Edition.Name, invoice.Registration.PassType.Name, participantName, issuer);

        using (MemoryStream pdfStream = new(pdfBytes))
        {
            invoice.PdfUrl = await _fileStorageService.SaveFileAsync(pdfStream, $"invoice-{invoice.Number}.pdf", cancellationToken);
        }

        _invoiceRepository.Update(invoice);
        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        return new InvoiceDto
        {
            Id = invoice.Id,
            RegistrationId = invoice.RegistrationId,
            Number = invoice.Number,
            IssuedAt = invoice.IssuedAt,
            Amount = invoice.Amount,
            BaseAmount = invoice.BaseAmount,
            VatRate = invoice.VatRate,
            VatAmount = invoice.VatAmount,
            PdfUrl = invoice.PdfUrl,
            FiscalName = invoice.FiscalName,
            TaxId = invoice.TaxId,
            Address = invoice.Address,
            City = invoice.City,
            PostalCode = invoice.PostalCode,
            Country = invoice.Country,
            CustomerFirstName = invoice.Registration.FirstName,
            CustomerLastName = invoice.Registration.LastName,
            CustomerEmail = invoice.Registration.Email,
            EditionId = invoice.Registration.EditionId,
            EditionName = invoice.Registration.Edition?.Name
        };
    }
}
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Files.Services;
using Alakai.FestivalManager.Application.Features.Invoices.Services;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;
using Alakai.FestivalManager.Domain.Enums;

namespace Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceHandler
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IInvoiceTemplateRepository _invoiceTemplateRepository;
    private readonly IInvoicePdfService _invoicePdfService;
    private readonly IFileStorageService _fileStorageService;

    public CreateInvoiceHandler(IInvoiceRepository invoiceRepository, IRegistrationRepository registrationRepository, IInvoiceTemplateRepository invoiceTemplateRepository, IInvoicePdfService invoicePdfService, IFileStorageService fileStorageService)
    {
        _invoiceRepository = invoiceRepository;
        _registrationRepository = registrationRepository;
        _invoiceTemplateRepository = invoiceTemplateRepository;
        _invoicePdfService = invoicePdfService;
        _fileStorageService = fileStorageService;
    }

    public async Task<InvoiceDto> HandleAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            throw new NotFoundException($"Registration with id '{command.RegistrationId}' was not found.");
        }

        if (registration.PaymentStatus != PaymentStatus.Paid)
        {
            throw new BusinessRuleException("An invoice can only be created for a registration that has been paid.");
        }

        Invoice? existing = await _invoiceRepository.GetByRegistrationIdAsync(command.RegistrationId, cancellationToken);

        if (existing is not null)
        {
            throw new BusinessRuleException("This registration already has an invoice.");
        }

        int year = DateTime.UtcNow.Year;
        int nextSequence = await _invoiceRepository.GetMaxSequenceNumberForYearAsync(year, cancellationToken) + 1;
        string number = $"{year}-{nextSequence:D4}";

        // FinalPrice is VAT-inclusive (what the customer actually paid). We break it
        // down into taxable base + VAT for the invoice, we do not add VAT on top.
        const decimal vatRate = 10m;
        decimal totalAmount = registration.FinalPrice;
        decimal baseAmount = Math.Round(totalAmount / (1 + vatRate / 100m), 2, MidpointRounding.AwayFromZero);
        decimal vatAmount = Math.Round(totalAmount - baseAmount, 2, MidpointRounding.AwayFromZero);

        Invoice invoice = new()
        {
            RegistrationId = registration.Id,
            Number = number,
            Year = year,
            SequenceNumber = nextSequence,
            IssuedAt = DateTime.UtcNow,
            Amount = totalAmount,
            BaseAmount = baseAmount,
            VatRate = vatRate,
            VatAmount = vatAmount,
            FiscalName = command.FiscalName,
            TaxId = command.TaxId,
            Address = command.Address,
            City = command.City,
            PostalCode = command.PostalCode,
            Country = command.Country
        };

        InvoiceTemplate? template = await _invoiceTemplateRepository.GetForEditionAsync(registration.EditionId, cancellationToken);

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

        string participantName = $"{registration.FirstName} {registration.LastName}";
        byte[] pdfBytes = _invoicePdfService.GenerateInvoicePdf(invoice, registration.Edition.Name, registration.PassType.Name, participantName, issuer);

        using (MemoryStream pdfStream = new(pdfBytes))
        {
            invoice.PdfUrl = await _fileStorageService.SaveFileAsync(pdfStream, $"invoice-{number}.pdf", cancellationToken);
        }

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
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
            CustomerFirstName = registration.FirstName,
            CustomerLastName = registration.LastName,
            CustomerEmail = registration.Email,
            EditionId = registration.EditionId,
            EditionName = registration.Edition.Name
        };
    }
}
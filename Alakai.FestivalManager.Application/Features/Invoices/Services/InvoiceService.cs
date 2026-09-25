using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoice;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.DeleteInvoice;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoice;
using Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;
using FluentValidation;

namespace Alakai.FestivalManager.Application.Features.Invoices.Services;

public class InvoiceService : IInvoiceService
{
    private readonly CreateInvoiceHandler _createInvoiceHandler;
    private readonly UpdateInvoiceHandler _updateInvoiceHandler;
    private readonly DeleteInvoiceHandler _deleteInvoiceHandler;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IValidator<CreateInvoiceCommand> _createInvoiceValidator;

    public InvoiceService(CreateInvoiceHandler createInvoiceHandler, UpdateInvoiceHandler updateInvoiceHandler, DeleteInvoiceHandler deleteInvoiceHandler, IInvoiceRepository invoiceRepository, IRegistrationRepository registrationRepository, IFileStorageService fileStorageService, IValidator<CreateInvoiceCommand> createInvoiceValidator)
    {
        _createInvoiceHandler = createInvoiceHandler;
        _updateInvoiceHandler = updateInvoiceHandler;
        _deleteInvoiceHandler = deleteInvoiceHandler;
        _invoiceRepository = invoiceRepository;
        _registrationRepository = registrationRepository;
        _fileStorageService = fileStorageService;
        _createInvoiceValidator = createInvoiceValidator;
    }

    public async Task<ApiResponse<UpdateInvoiceResponse>> UpdateAsync(UpdateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        InvoiceDto invoiceDto = await _updateInvoiceHandler.HandleAsync(command, cancellationToken);
        return new ApiResponse<UpdateInvoiceResponse> { Success = true, Data = new UpdateInvoiceResponse { Invoice = invoiceDto }, Errors = [], Message = $"Invoice {invoiceDto.Number} updated successfully." };
    }

    public async Task<ApiResponse<DeleteInvoiceResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _deleteInvoiceHandler.HandleAsync(new DeleteInvoiceCommand { Id = id }, cancellationToken);
        return new ApiResponse<DeleteInvoiceResponse> { Success = true, Data = new DeleteInvoiceResponse(), Errors = [], Message = "Invoice deleted successfully." };
    }

    public async Task<ApiResponse<CreateInvoiceResponse>> CreateAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        await _createInvoiceValidator.ValidateAndThrowAsync(command, cancellationToken);
        InvoiceDto invoiceDto = await _createInvoiceHandler.HandleAsync(command, cancellationToken);
        return new ApiResponse<CreateInvoiceResponse> { Success = true, Data = new CreateInvoiceResponse { Invoice = invoiceDto }, Errors = [], Message = $"Invoice {invoiceDto.Number} created successfully." };
    }

    public async Task<ApiResponse<GetInvoicesResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Invoice> invoices = await _invoiceRepository.GetAllAsync(cancellationToken);

        List<InvoiceDto> invoiceDtos = invoices.Select(i => new InvoiceDto
        {
            Id = i.Id,
            RegistrationId = i.RegistrationId,
            Number = i.Number,
            IssuedAt = i.IssuedAt,
            Amount = i.Amount,
            BaseAmount = i.BaseAmount,
            VatRate = i.VatRate,
            VatAmount = i.VatAmount,
            PdfUrl = i.PdfUrl,
            FiscalName = i.FiscalName,
            TaxId = i.TaxId,
            Address = i.Address,
            City = i.City,
            PostalCode = i.PostalCode,
            Country = i.Country,
            CustomerFirstName = i.Registration.FirstName,
            CustomerLastName = i.Registration.LastName,
            CustomerEmail = i.Registration.Email,
            EditionId = i.Registration.EditionId,
            EditionName = i.Registration.Edition != null ? i.Registration.Edition.Name : null
        }).ToList();

        return new ApiResponse<GetInvoicesResponse> { Success = true, Data = new GetInvoicesResponse { Invoices = invoiceDtos }, Errors = [], Message = $"There are {invoiceDtos.Count} invoices." };
    }

    public async Task<byte[]> GetInvoicesZipForEditionAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await BuildZipForEditionAsync(editionId, cancellationToken);
    }

    public async Task<byte[]> BulkCreateAndZipInvoicesForEditionAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Registration> registrations = await _registrationRepository.GetByEditionIdAsync(editionId, cancellationToken);
        IReadOnlyList<Invoice> existingInvoices = await _invoiceRepository.GetByEditionIdAsync(editionId, cancellationToken);
        HashSet<Guid> registrationsWithInvoice = existingInvoices.Select(i => i.RegistrationId).ToHashSet();

        foreach (Registration registration in registrations)
        {
            if (registration.PaymentStatus != PaymentStatus.Paid || registrationsWithInvoice.Contains(registration.Id))
            {
                continue;
            }

            // Esta factura la genera el admin en bloque para uso interno (contabilidad
            // propia), no porque el asistente la haya pedido desde su panel -- por eso
            // no exigimos NIF ni direccion como en la creacion individual. Usamos los
            // datos que ya tenemos del propio registro; lo que no tenemos (direccion,
            // codigo postal, porque el formulario de inscripcion nunca los pide) se
            // deja en blanco, y la plantilla del PDF ya oculta esas lineas si estan
            // vacias (ver QuestPdfInvoiceService).
            CreateInvoiceCommand command = new()
            {
                RegistrationId = registration.Id,
                FiscalName = $"{registration.FirstName} {registration.LastName}".Trim(),
                TaxId = registration.DocumentNumber ?? string.Empty,
                Address = string.Empty,
                City = registration.City ?? string.Empty,
                PostalCode = string.Empty,
                Country = registration.Country ?? string.Empty
            };

            await _createInvoiceHandler.HandleAsync(command, cancellationToken);
        }

        return await BuildZipForEditionAsync(editionId, cancellationToken);
    }

    private async Task<byte[]> BuildZipForEditionAsync(Guid editionId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Invoice> invoices = await _invoiceRepository.GetByEditionIdAsync(editionId, cancellationToken);

        using MemoryStream zipStream = new();

        using (ZipArchive archive = new(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (Invoice invoice in invoices)
            {
                byte[]? pdfBytes = await _fileStorageService.TryDownloadAsync(invoice.PdfUrl, cancellationToken);

                if (pdfBytes is null)
                {
                    continue;
                }

                ZipArchiveEntry entry = archive.CreateEntry($"invoice-{invoice.Number}.pdf", CompressionLevel.Fastest);
                await using Stream entryStream = entry.Open();
                await entryStream.WriteAsync(pdfBytes, cancellationToken);
            }
        }

        return zipStream.ToArray();
    }
}
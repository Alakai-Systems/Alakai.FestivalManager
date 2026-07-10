using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoice;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;
using FluentValidation;

namespace Alakai.FestivalManager.Application.Features.Invoices.Services;

public class InvoiceService : IInvoiceService
{
    private readonly CreateInvoiceHandler _createInvoiceHandler;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IValidator<CreateInvoiceCommand> _createInvoiceValidator;

    public InvoiceService(CreateInvoiceHandler createInvoiceHandler, IInvoiceRepository invoiceRepository, IValidator<CreateInvoiceCommand> createInvoiceValidator)
    {
        _createInvoiceHandler = createInvoiceHandler;
        _invoiceRepository = invoiceRepository;
        _createInvoiceValidator = createInvoiceValidator;
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
            CustomerFirstName = i.Registration.FirstName,
            CustomerLastName = i.Registration.LastName,
            CustomerEmail = i.Registration.Email,
            EditionId = i.Registration.EditionId,
            EditionName = i.Registration.Edition != null ? i.Registration.Edition.Name : null
        }).ToList();

        return new ApiResponse<GetInvoicesResponse> { Success = true, Data = new GetInvoicesResponse { Invoices = invoiceDtos }, Errors = [], Message = $"There are {invoiceDtos.Count} invoices." };
    }
}
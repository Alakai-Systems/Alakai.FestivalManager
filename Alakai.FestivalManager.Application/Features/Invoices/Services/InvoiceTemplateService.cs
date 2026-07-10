using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoiceTemplate;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoiceTemplate;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Invoices.Services;

public class InvoiceTemplateService : IInvoiceTemplateService
{
    private readonly IInvoiceTemplateRepository _invoiceTemplateRepository;
    private readonly CreateInvoiceTemplateHandler _createInvoiceTemplateHandler;
    private readonly UpdateInvoiceTemplateHandler _updateInvoiceTemplateHandler;

    public InvoiceTemplateService(IInvoiceTemplateRepository invoiceTemplateRepository, CreateInvoiceTemplateHandler createInvoiceTemplateHandler, UpdateInvoiceTemplateHandler updateInvoiceTemplateHandler)
    {
        _invoiceTemplateRepository = invoiceTemplateRepository;
        _createInvoiceTemplateHandler = createInvoiceTemplateHandler;
        _updateInvoiceTemplateHandler = updateInvoiceTemplateHandler;
    }

    public async Task<ApiResponse<GetInvoiceTemplatesResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<InvoiceTemplate> templates = await _invoiceTemplateRepository.GetAllAsync(cancellationToken);

        List<InvoiceTemplateDto> dtos = templates.Select(t => new InvoiceTemplateDto
        {
            Id = t.Id,
            EditionId = t.EditionId,
            EditionName = t.Edition?.Name,
            Name = t.Name,
            CompanyName = t.CompanyName,
            TaxId = t.TaxId,
            Address = t.Address,
            City = t.City,
            PostalCode = t.PostalCode,
            Country = t.Country,
            LogoUrl = t.LogoUrl,
            IsActive = t.IsActive
        }).ToList();

        return new ApiResponse<GetInvoiceTemplatesResponse> { Success = true, Data = new GetInvoiceTemplatesResponse { Templates = dtos }, Errors = [], Message = $"There are {dtos.Count} invoice templates." };
    }

    public async Task<ApiResponse<CreateInvoiceTemplateResponse>> CreateAsync(CreateInvoiceTemplateCommand command, CancellationToken cancellationToken = default)
    {
        InvoiceTemplateDto dto = await _createInvoiceTemplateHandler.HandleAsync(command, cancellationToken);
        return new ApiResponse<CreateInvoiceTemplateResponse> { Success = true, Data = new CreateInvoiceTemplateResponse { Template = dto }, Errors = [], Message = $"Template '{dto.Name}' created successfully." };
    }

    public async Task<ApiResponse<UpdateInvoiceTemplateResponse>> UpdateAsync(UpdateInvoiceTemplateCommand command, CancellationToken cancellationToken = default)
    {
        InvoiceTemplateDto dto = await _updateInvoiceTemplateHandler.HandleAsync(command, cancellationToken);
        return new ApiResponse<UpdateInvoiceTemplateResponse> { Success = true, Data = new UpdateInvoiceTemplateResponse { Template = dto }, Errors = [], Message = $"Template '{dto.Name}' updated successfully." };
    }

    public async Task<ApiResponse<DeleteInvoiceTemplateResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        InvoiceTemplate? template = await _invoiceTemplateRepository.GetByIdAsync(id, cancellationToken);

        if (template is null)
        {
            return new ApiResponse<DeleteInvoiceTemplateResponse> { Success = true, Data = new DeleteInvoiceTemplateResponse { Id = id, Deleted = false }, Errors = [], Message = "Template not found." };
        }

        _invoiceTemplateRepository.Delete(template);
        await _invoiceTemplateRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<DeleteInvoiceTemplateResponse> { Success = true, Data = new DeleteInvoiceTemplateResponse { Id = id, Deleted = true }, Errors = [], Message = "Template deleted successfully." };
    }
}
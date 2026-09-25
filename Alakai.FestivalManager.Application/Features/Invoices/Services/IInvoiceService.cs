using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Invoices.Commands.CreateInvoice;

namespace Alakai.FestivalManager.Application.Features.Invoices.Services;

public interface IInvoiceService
{
    Task<ApiResponse<CreateInvoiceResponse>> CreateAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetInvoicesResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses.UpdateInvoiceResponse>> UpdateAsync(Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoice.UpdateInvoiceCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<Alakai.FestivalManager.Application.Features.Invoices.Contracts.Responses.DeleteInvoiceResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>ZIP con los PDF de todas las facturas ya existentes de una edicion (no crea ninguna).</summary>
    Task<byte[]> GetInvoicesZipForEditionAsync(Guid editionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea la factura que falte (registros Paid sin factura todavia) para cada
    /// asistente de la edicion, usando los datos del propio registro (sin NIF ni
    /// direccion, que el formulario de inscripcion nunca pide), y devuelve el ZIP
    /// con TODAS las facturas de la edicion (las que ya existian + las nuevas).
    /// </summary>
    Task<byte[]> BulkCreateAndZipInvoicesForEditionAsync(Guid editionId, CancellationToken cancellationToken = default);
}
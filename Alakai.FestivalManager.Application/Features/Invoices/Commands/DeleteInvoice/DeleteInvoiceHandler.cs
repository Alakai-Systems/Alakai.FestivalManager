using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Invoices.Commands.DeleteInvoice;

public class DeleteInvoiceHandler
{
    private readonly IInvoiceRepository _invoiceRepository;

    public DeleteInvoiceHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task HandleAsync(DeleteInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        Invoice? invoice = await _invoiceRepository.GetByIdAsync(command.Id, cancellationToken);

        if (invoice is null)
        {
            throw new NotFoundException($"Invoice with id '{command.Id}' was not found.");
        }

        _invoiceRepository.Delete(invoice);
        await _invoiceRepository.SaveChangesAsync(cancellationToken);
    }
}
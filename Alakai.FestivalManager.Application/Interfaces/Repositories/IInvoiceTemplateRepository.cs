namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IInvoiceTemplateRepository
{
    Task AddAsync(InvoiceTemplate template, CancellationToken cancellationToken = default);
    Task<InvoiceTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvoiceTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InvoiceTemplate?> GetForEditionAsync(Guid editionId, CancellationToken cancellationToken = default);
    void Delete(InvoiceTemplate template);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
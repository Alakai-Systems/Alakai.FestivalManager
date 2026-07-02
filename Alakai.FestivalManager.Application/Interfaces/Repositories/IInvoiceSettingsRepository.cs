namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IInvoiceSettingsRepository
{
    Task<InvoiceSettings?> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(InvoiceSettings settings, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
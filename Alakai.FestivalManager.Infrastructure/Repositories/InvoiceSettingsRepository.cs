namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class InvoiceSettingsRepository : IInvoiceSettingsRepository
{
    private readonly FestivalManagerDbContext _context;

    public InvoiceSettingsRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceSettings?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InvoiceSettings.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(InvoiceSettings settings, CancellationToken cancellationToken = default)
    {
        await _context.InvoiceSettings.AddAsync(settings, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
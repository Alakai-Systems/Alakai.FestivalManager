namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly FestivalManagerDbContext _context;

    public InvoiceRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _context.Invoices.AddAsync(invoice, cancellationToken);
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Registration).ThenInclude(r => r.Edition)
            .Include(i => i.Registration).ThenInclude(r => r.PassType)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .FirstOrDefaultAsync(i => i.RegistrationId == registrationId, cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Include(i => i.Registration)
                .ThenInclude(r => r.Edition)
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetMaxSequenceNumberForYearAsync(int year, CancellationToken cancellationToken = default)
    {
        bool any = await _context.Invoices.AnyAsync(i => i.Year == year, cancellationToken);

        if (!any)
        {
            return 0;
        }

        return await _context.Invoices
            .Where(i => i.Year == year)
            .MaxAsync(i => i.SequenceNumber, cancellationToken);
    }

    public void Update(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
    }

    public void Delete(Invoice invoice)
    {
        _context.Invoices.Remove(invoice);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class InvoiceTemplateRepository : IInvoiceTemplateRepository
{
    private readonly FestivalManagerDbContext _context;

    public InvoiceTemplateRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(InvoiceTemplate template, CancellationToken cancellationToken = default)
    {
        await _context.InvoiceTemplates.AddAsync(template, cancellationToken);
    }

    public async Task<InvoiceTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.InvoiceTemplates
            .Include(t => t.Edition)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<InvoiceTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.InvoiceTemplates
            .Include(t => t.Edition)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<InvoiceTemplate?> GetForEditionAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        InvoiceTemplate? specific = await _context.InvoiceTemplates
            .FirstOrDefaultAsync(t => t.EditionId == editionId && t.IsActive, cancellationToken);

        if (specific is not null)
        {
            return specific;
        }

        return await _context.InvoiceTemplates
            .FirstOrDefaultAsync(t => t.EditionId == null && t.IsActive, cancellationToken);
    }

    public void Delete(InvoiceTemplate template)
    {
        _context.InvoiceTemplates.Remove(template);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
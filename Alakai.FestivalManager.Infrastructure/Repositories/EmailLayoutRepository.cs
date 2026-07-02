namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class EmailLayoutRepository : IEmailLayoutRepository
{
    private readonly FestivalManagerDbContext _context;

    public EmailLayoutRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<EmailLayout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLayouts
            .Include(e => e.Edition)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<EmailLayout>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailLayouts
            .Include(e => e.Edition)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailLayout?> GetForEditionAsync(Guid? editionId, CancellationToken cancellationToken = default)
    {
        if (editionId.HasValue)
        {
            EmailLayout? specific = await _context.EmailLayouts
                .FirstOrDefaultAsync(e => e.EditionId == editionId.Value && e.IsActive, cancellationToken);

            if (specific is not null)
            {
                return specific;
            }
        }

        return await _context.EmailLayouts
            .FirstOrDefaultAsync(e => e.EditionId == null && e.IsActive, cancellationToken);
    }

    public async Task AddAsync(EmailLayout emailLayout, CancellationToken cancellationToken = default)
    {
        await _context.EmailLayouts.AddAsync(emailLayout, cancellationToken);
    }

    public void Update(EmailLayout emailLayout)
    {
        _context.EmailLayouts.Update(emailLayout);
    }

    public void Delete(EmailLayout emailLayout)
    {
        _context.EmailLayouts.Remove(emailLayout);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
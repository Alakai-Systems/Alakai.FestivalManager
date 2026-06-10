namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class EditionRepository : IEditionRepository
{
    private readonly FestivalManagerDbContext _context;

    public EditionRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Edition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Editions.AsNoTracking().OrderByDescending(e => e.Year).ToListAsync(cancellationToken);
    }

    public async Task<Edition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Editions.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Edition>> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default)
    {
        return await _context.Editions.AsNoTracking().Where(e => e.FestivalId == festivalId).OrderByDescending(e => e.Year).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByFestivalAndYearAsync(Guid festivalId, int year, CancellationToken cancellationToken = default)
    {
        return await _context.Editions.AnyAsync(e => e.FestivalId == festivalId && e.Year == year, cancellationToken);
    }

    public async Task AddAsync(Edition edition, CancellationToken cancellationToken = default)
    {
        await _context.Editions.AddAsync(edition, cancellationToken);
    }

    public void Update(Edition edition)
    {
        _context.Editions.Update(edition);
    }

    public void Delete(Edition edition)
    {
        _context.Editions.Remove(edition);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
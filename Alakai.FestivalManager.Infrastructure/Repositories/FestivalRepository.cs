namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class FestivalRepository : IFestivalRepository
{
    private readonly FestivalManagerDbContext _context;

    public FestivalRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Festival>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Festivals
            .AsNoTracking()
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Festival?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Festivals
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Festival?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Festivals
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Slug == slug, cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Festivals
            .AnyAsync(f => f.Slug == slug, cancellationToken);
    }

    public async Task AddAsync(Festival festival, CancellationToken cancellationToken = default)
    {
        await _context.Festivals.AddAsync(festival, cancellationToken);
    }

    public void Update(Festival festival)
    {
        _context.Festivals.Update(festival);
    }

    public void Delete(Festival festival)
    {
        _context.Festivals.Remove(festival);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
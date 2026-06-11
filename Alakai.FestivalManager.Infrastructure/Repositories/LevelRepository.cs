namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class LevelRepository : ILevelRepository
{
    private readonly FestivalManagerDbContext _context;

    public LevelRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Level>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Levels.AsNoTracking().OrderBy(l => l.SortOrder).ThenBy(l => l.Name).ToListAsync(cancellationToken);
    }

    public async Task<Level?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Levels.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Level>> GetByPassTypeIdAsync(Guid passTypeId, CancellationToken cancellationToken = default)
    {
        return await _context.Levels.AsNoTracking().Where(l => l.PassTypeId == passTypeId).OrderBy(l => l.SortOrder).ThenBy(l => l.Name).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByPassTypeAndNameAsync(Guid passTypeId, string name, CancellationToken cancellationToken = default)
    {
        return await _context.Levels.AnyAsync(l => l.PassTypeId == passTypeId && l.Name == name, cancellationToken);
    }

    public async Task AddAsync(Level level, CancellationToken cancellationToken = default)
    {
        await _context.Levels.AddAsync(level, cancellationToken);
    }

    public void Update(Level level)
    {
        _context.Levels.Update(level);
    }

    public void Delete(Level level)
    {
        _context.Levels.Remove(level);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
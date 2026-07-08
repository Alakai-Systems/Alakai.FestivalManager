namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class PassTypeRepository : IPassTypeRepository
{
    private readonly FestivalManagerDbContext _context;

    public PassTypeRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PassType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PassTypes.AsNoTracking().OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public async Task<PassType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PassTypes.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PassType>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.PassTypes.AsNoTracking().Where(p => p.EditionId == editionId).OrderBy(p => p.SortOrder).ThenBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PassType>> GetActiveByEditionIdWithLevelsAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.PassTypes
            .AsNoTracking()
            .Include(p => p.Levels)
            .Where(p => p.EditionId == editionId && p.IsActive)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEditionAndNameAsync(Guid editionId, string name, CancellationToken cancellationToken = default)
    {
        return await _context.PassTypes.AnyAsync(p => p.EditionId == editionId && p.Name == name, cancellationToken);
    }

    public async Task AddAsync(PassType passType, CancellationToken cancellationToken = default)
    {
        await _context.PassTypes.AddAsync(passType, cancellationToken);
    }

    public void Update(PassType passType)
    {
        _context.PassTypes.Update(passType);
    }

    public void Delete(PassType passType)
    {
        _context.PassTypes.Remove(passType);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

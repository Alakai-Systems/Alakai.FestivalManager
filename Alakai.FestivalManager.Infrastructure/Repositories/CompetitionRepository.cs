namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class CompetitionRepository : ICompetitionRepository
{
    private readonly FestivalManagerDbContext _context;

    public CompetitionRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Competition competition, CancellationToken cancellationToken = default)
    {
        await _context.Competitions.AddAsync(competition, cancellationToken);
    }

    public async Task<Competition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Competitions
            .Include(c => c.Capacities)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Competition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Competitions
            .Include(c => c.Capacities)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Competition>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.Competitions
            .Include(c => c.Capacities)
            .Where(c => c.EditionId == editionId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEditionAndNameAsync(Guid editionId, string name, CancellationToken cancellationToken = default)
    {
        return await _context.Competitions.AnyAsync(c => c.EditionId == editionId && c.Name == name, cancellationToken);
    }

    public void Update(Competition competition)
    {
        _context.Competitions.Update(competition);
    }

    public void Delete(Competition competition)
    {
        _context.Competitions.Remove(competition);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

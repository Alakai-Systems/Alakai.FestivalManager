namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class CompetitionCapacityRepository : ICompetitionCapacityRepository
{
    private readonly FestivalManagerDbContext _context;

    public CompetitionCapacityRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CompetitionCapacity capacity, CancellationToken cancellationToken = default)
    {
        await _context.CompetitionCapacities.AddAsync(capacity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<CompetitionCapacity> capacities, CancellationToken cancellationToken = default)
    {
        await _context.CompetitionCapacities.AddRangeAsync(capacities, cancellationToken);
    }

    public async Task<CompetitionCapacity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionCapacities.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CompetitionCapacity>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionCapacities
            .Where(c => c.CompetitionId == competitionId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompetitionCapacity>> GetByCompetitionIdsAsync(IReadOnlyList<Guid> competitionIds, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionCapacities
            .Where(c => competitionIds.Contains(c.CompetitionId) && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public void Update(CompetitionCapacity capacity)
    {
        _context.CompetitionCapacities.Update(capacity);
    }

    public void Delete(CompetitionCapacity capacity)
    {
        _context.CompetitionCapacities.Remove(capacity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

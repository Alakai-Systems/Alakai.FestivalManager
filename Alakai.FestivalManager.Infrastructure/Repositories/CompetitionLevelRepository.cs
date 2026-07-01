namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class CompetitionLevelRepository : ICompetitionLevelRepository
{
    private readonly FestivalManagerDbContext _context;

    public CompetitionLevelRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CompetitionLevel level, CancellationToken cancellationToken = default)
    {
        await _context.CompetitionLevels.AddAsync(level, cancellationToken);
    }

    public async Task<CompetitionLevel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionLevels.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CompetitionLevel>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionLevels
            .Where(l => l.CompetitionId == competitionId)
            .OrderBy(l => l.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public void Update(CompetitionLevel level)
    {
        _context.CompetitionLevels.Update(level);
    }

    public void Delete(CompetitionLevel level)
    {
        _context.CompetitionLevels.Remove(level);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class CompetitionEntryRepository : ICompetitionEntryRepository
{
    private readonly FestivalManagerDbContext _context;

    public CompetitionEntryRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CompetitionEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.CompetitionEntries.AddAsync(entry, cancellationToken);
    }

    public async Task<CompetitionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionEntries.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CompetitionEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionEntries.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompetitionEntry>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionEntries.Where(e => e.CompetitionId == competitionId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompetitionEntry>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionEntries.Where(e => e.RegistrationId == registrationId).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCompetitionAndRegistrationAsync(Guid competitionId, Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionEntries.AnyAsync(e => e.CompetitionId == competitionId && e.RegistrationId == registrationId, cancellationToken);
    }

    public void Update(CompetitionEntry entry)
    {
        _context.CompetitionEntries.Update(entry);
    }

    public void Delete(CompetitionEntry entry)
    {
        _context.CompetitionEntries.Remove(entry);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

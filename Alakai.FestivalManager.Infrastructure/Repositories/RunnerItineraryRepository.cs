namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class RunnerItineraryRepository : IRunnerItineraryRepository
{
    private readonly FestivalManagerDbContext _context;

    public RunnerItineraryRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<RunnerItinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RunnerItineraries
            .Include(i => i.Trips).ThenInclude(t => t.ProductionPerson)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<RunnerItinerary>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.RunnerItineraries
            .AsNoTracking()
            .Include(i => i.Trips).ThenInclude(t => t.ProductionPerson)
            .Where(i => i.EditionId == editionId)
            .OrderBy(i => i.DateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RunnerItinerary itinerary, CancellationToken cancellationToken = default)
    {
        await _context.RunnerItineraries.AddAsync(itinerary, cancellationToken);
    }

    public void Delete(RunnerItinerary itinerary)
    {
        _context.RunnerItineraries.Remove(itinerary);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
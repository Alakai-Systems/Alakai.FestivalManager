namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class ProductionTripRepository : IProductionTripRepository
{
    private readonly FestivalManagerDbContext _context;

    public ProductionTripRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<ProductionTrip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionTrips
            .Include(t => t.ProductionPerson)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductionTrip>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionTrips
            .AsNoTracking()
            .Include(t => t.ProductionPerson)
            .Where(t => t.EditionId == editionId)
            .OrderBy(t => t.DateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProductionTrip trip, CancellationToken cancellationToken = default)
    {
        await _context.ProductionTrips.AddAsync(trip, cancellationToken);
    }

    public void Delete(ProductionTrip trip)
    {
        _context.ProductionTrips.Remove(trip);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class ProductionAccommodationZoneRepository : IProductionAccommodationZoneRepository
{
    private readonly FestivalManagerDbContext _context;

    public ProductionAccommodationZoneRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductionAccommodationZone>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodationZones.AsNoTracking().OrderBy(z => z.SortOrder).ThenBy(z => z.Name).ToListAsync(cancellationToken);
    }

    public async Task<ProductionAccommodationZone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodationZones.FirstOrDefaultAsync(z => z.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductionAccommodationZone>> GetByBuildingIdAsync(Guid productionAccommodationBuildingId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodationZones.AsNoTracking().Where(z => z.ProductionAccommodationBuildingId == productionAccommodationBuildingId).OrderBy(z => z.SortOrder).ThenBy(z => z.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProductionAccommodationZone zone, CancellationToken cancellationToken = default)
    {
        await _context.ProductionAccommodationZones.AddAsync(zone, cancellationToken);
    }

    public void Update(ProductionAccommodationZone zone)
    {
        _context.ProductionAccommodationZones.Update(zone);
    }

    public void Delete(ProductionAccommodationZone zone)
    {
        _context.ProductionAccommodationZones.Remove(zone);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
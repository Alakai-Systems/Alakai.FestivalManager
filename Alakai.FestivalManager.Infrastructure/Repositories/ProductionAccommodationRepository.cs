namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class ProductionAccommodationRepository : IProductionAccommodationRepository
{
    private readonly FestivalManagerDbContext _context;

    public ProductionAccommodationRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductionAccommodation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodations.AsNoTracking().OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToListAsync(cancellationToken);
    }

    public async Task<ProductionAccommodation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodations.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductionAccommodation>> GetByZoneIdAsync(Guid productionAccommodationZoneId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodations.AsNoTracking().Where(a => a.ProductionAccommodationZoneId == productionAccommodationZoneId).OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProductionAccommodation accommodation, CancellationToken cancellationToken = default)
    {
        await _context.ProductionAccommodations.AddAsync(accommodation, cancellationToken);
    }

    public void Update(ProductionAccommodation accommodation)
    {
        _context.ProductionAccommodations.Update(accommodation);
    }

    public void Delete(ProductionAccommodation accommodation)
    {
        _context.ProductionAccommodations.Remove(accommodation);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
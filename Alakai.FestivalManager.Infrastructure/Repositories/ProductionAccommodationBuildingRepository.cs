namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class ProductionAccommodationBuildingRepository : IProductionAccommodationBuildingRepository
{
    private readonly FestivalManagerDbContext _context;

    public ProductionAccommodationBuildingRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductionAccommodationBuilding>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodationBuildings.AsNoTracking().OrderBy(b => b.SortOrder).ThenBy(b => b.Name).ToListAsync(cancellationToken);
    }

    public async Task<ProductionAccommodationBuilding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodationBuildings.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductionAccommodationBuilding>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodationBuildings.AsNoTracking().Where(b => b.EditionId == editionId).OrderBy(b => b.SortOrder).ThenBy(b => b.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProductionAccommodationBuilding building, CancellationToken cancellationToken = default)
    {
        await _context.ProductionAccommodationBuildings.AddAsync(building, cancellationToken);
    }

    public void Update(ProductionAccommodationBuilding building)
    {
        _context.ProductionAccommodationBuildings.Update(building);
    }

    public void Delete(ProductionAccommodationBuilding building)
    {
        _context.ProductionAccommodationBuildings.Remove(building);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
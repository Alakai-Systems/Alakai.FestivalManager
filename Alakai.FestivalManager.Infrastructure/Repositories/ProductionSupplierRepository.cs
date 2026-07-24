namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class ProductionSupplierRepository : IProductionSupplierRepository
{
    private readonly FestivalManagerDbContext _context;

    public ProductionSupplierRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductionSupplier>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProductionSuppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync(cancellationToken);
    }

    public async Task<ProductionSupplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionSuppliers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductionSupplier>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionSuppliers.AsNoTracking().Where(s => s.EditionId == editionId).OrderBy(s => s.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProductionSupplier productionSupplier, CancellationToken cancellationToken = default)
    {
        await _context.ProductionSuppliers.AddAsync(productionSupplier, cancellationToken);
    }

    public void Update(ProductionSupplier productionSupplier)
    {
        _context.ProductionSuppliers.Update(productionSupplier);
    }

    public void Delete(ProductionSupplier productionSupplier)
    {
        _context.ProductionSuppliers.Remove(productionSupplier);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
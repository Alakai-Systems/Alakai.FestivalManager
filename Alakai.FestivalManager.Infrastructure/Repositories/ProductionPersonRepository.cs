namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class ProductionPersonRepository : IProductionPersonRepository
{
    private readonly FestivalManagerDbContext _context;

    public ProductionPersonRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductionPerson>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProductionPeople.AsNoTracking().OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToListAsync(cancellationToken);
    }

    public async Task<ProductionPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionPeople.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductionPerson>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionPeople.AsNoTracking().Where(p => p.EditionId == editionId).OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEditionAndDocumentNumberAsync(Guid editionId, string documentNumber, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionPeople.AnyAsync(p => p.EditionId == editionId && p.DocumentNumber == documentNumber, cancellationToken);
    }

    public async Task AddAsync(ProductionPerson productionPerson, CancellationToken cancellationToken = default)
    {
        await _context.ProductionPeople.AddAsync(productionPerson, cancellationToken);
    }

    public void Update(ProductionPerson productionPerson)
    {
        _context.ProductionPeople.Update(productionPerson);
    }

    public void Delete(ProductionPerson productionPerson)
    {
        _context.ProductionPeople.Remove(productionPerson);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
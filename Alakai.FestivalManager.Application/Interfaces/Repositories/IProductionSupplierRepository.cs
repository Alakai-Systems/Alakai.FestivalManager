namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IProductionSupplierRepository
{
    Task<IReadOnlyList<ProductionSupplier>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductionSupplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductionSupplier>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductionSupplier productionSupplier, CancellationToken cancellationToken = default);
    void Update(ProductionSupplier productionSupplier);
    void Delete(ProductionSupplier productionSupplier);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IProductionTripRepository
{
    Task<ProductionTrip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductionTrip>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductionTrip trip, CancellationToken cancellationToken = default);
    void Delete(ProductionTrip trip);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
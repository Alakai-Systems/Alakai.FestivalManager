namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IProductionAccommodationRepository
{
    Task<IReadOnlyList<ProductionAccommodation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductionAccommodation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductionAccommodation>> GetByZoneIdAsync(Guid productionAccommodationZoneId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductionAccommodation accommodation, CancellationToken cancellationToken = default);
    void Update(ProductionAccommodation accommodation);
    void Delete(ProductionAccommodation accommodation);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
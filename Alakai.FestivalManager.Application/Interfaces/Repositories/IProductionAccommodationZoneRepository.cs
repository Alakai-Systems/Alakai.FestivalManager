namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IProductionAccommodationZoneRepository
{
    Task<IReadOnlyList<ProductionAccommodationZone>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductionAccommodationZone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductionAccommodationZone>> GetByBuildingIdAsync(Guid productionAccommodationBuildingId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductionAccommodationZone zone, CancellationToken cancellationToken = default);
    void Update(ProductionAccommodationZone zone);
    void Delete(ProductionAccommodationZone zone);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
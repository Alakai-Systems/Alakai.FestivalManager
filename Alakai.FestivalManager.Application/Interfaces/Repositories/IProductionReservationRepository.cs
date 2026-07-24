namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IProductionReservationRepository
{
    Task<IReadOnlyList<ProductionAccommodationReservation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductionAccommodationReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductionAccommodationReservation>> GetByBuildingIdAsync(Guid productionAccommodationBuildingId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductionAccommodationReservation reservation, CancellationToken cancellationToken = default);
    void Delete(ProductionAccommodationReservation reservation);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
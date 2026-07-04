namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IBusReservationRepository
{
    Task<BusReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BusReservation>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BusReservation>> GetByBusIdAsync(Guid busId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BusReservation>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<int> GetOccupiedCountAsync(Guid busId, CancellationToken cancellationToken = default);
    Task<bool> HasReservationForDirectionAsync(Guid registrationId, BusDirection direction, Guid? excludeReservationId, CancellationToken cancellationToken = default);
    Task AddAsync(BusReservation reservation, CancellationToken cancellationToken = default);
    void Delete(BusReservation reservation);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IAccommodationReservationRepository
{
    Task<AccommodationReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccommodationReservation>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default);
    Task<AccommodationReservation?> GetByResponsibleRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<AccommodationReservation?> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<bool> IsRegistrationAlreadyBookedAsync(Guid editionId, Guid registrationId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetOccupancyCountsAsync(IReadOnlyList<Guid> accommodationIds, CancellationToken cancellationToken = default);
    Task AddAsync(AccommodationReservation reservation, CancellationToken cancellationToken = default);
    void Delete(AccommodationReservation reservation);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
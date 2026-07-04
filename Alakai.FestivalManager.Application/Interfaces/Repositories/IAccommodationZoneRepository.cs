namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IAccommodationZoneRepository
{
    Task<AccommodationZone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(AccommodationZone zone, CancellationToken cancellationToken = default);
    void Update(AccommodationZone zone);
    void Delete(AccommodationZone zone);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
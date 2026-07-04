namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IAccommodationRepository
{
    Task<Accommodation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Accommodation accommodation, CancellationToken cancellationToken = default);
    void Update(Accommodation accommodation);
    void Delete(Accommodation accommodation);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
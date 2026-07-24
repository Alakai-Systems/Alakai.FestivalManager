namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IRunnerItineraryRepository
{
    Task<RunnerItinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RunnerItinerary>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task AddAsync(RunnerItinerary itinerary, CancellationToken cancellationToken = default);
    void Delete(RunnerItinerary itinerary);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
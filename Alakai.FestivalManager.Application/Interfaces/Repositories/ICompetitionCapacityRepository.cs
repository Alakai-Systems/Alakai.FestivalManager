namespace Alakai.FestivalManager.Application.Contracts.Repositories;

public interface ICompetitionCapacityRepository
{
    Task AddAsync(CompetitionCapacity capacity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<CompetitionCapacity> capacities, CancellationToken cancellationToken = default);
    Task<CompetitionCapacity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionCapacity>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<CompetitionCapacity?> GetByCompetitionLevelAndRoleAsync(Guid competitionId, MixAndMatchLevel? mixAndMatchLevel, DanceRole danceRole, CancellationToken cancellationToken = default);
    void Update(CompetitionCapacity capacity);
    void Delete(CompetitionCapacity capacity);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

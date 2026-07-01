namespace Alakai.FestivalManager.Application.Contracts.Repositories;

public interface ICompetitionLevelRepository
{
    Task AddAsync(CompetitionLevel level, CancellationToken cancellationToken = default);
    Task<CompetitionLevel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionLevel>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);
    void Update(CompetitionLevel level);
    void Delete(CompetitionLevel level);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

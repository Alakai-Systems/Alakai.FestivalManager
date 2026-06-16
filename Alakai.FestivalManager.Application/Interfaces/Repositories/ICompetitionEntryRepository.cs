namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface ICompetitionEntryRepository
{
    Task AddAsync(CompetitionEntry entry, CancellationToken cancellationToken = default);
    Task<CompetitionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionEntry>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionEntry>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCompetitionAndRegistrationAsync(Guid competitionId, Guid registrationId, CancellationToken cancellationToken = default);
    void Update(CompetitionEntry entry);
    void Delete(CompetitionEntry entry);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

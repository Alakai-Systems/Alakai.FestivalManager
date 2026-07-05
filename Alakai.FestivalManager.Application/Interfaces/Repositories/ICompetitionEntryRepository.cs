using Alakai.FestivalManager.Domain.Enums;

namespace Alakai.FestivalManager.Application.Contracts.Repositories;

public interface ICompetitionEntryRepository
{
    Task AddAsync(CompetitionEntry entry, CancellationToken cancellationToken = default);
    Task<CompetitionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionEntry>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionEntry>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionEntry>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCompetitionAndRegistrationAsync(Guid competitionId, Guid registrationId, CancellationToken cancellationToken = default);
    Task<int> CountActiveByCapacityIdAsync(Guid capacityId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCapacityIdAsync(Guid capacityId, CancellationToken cancellationToken);

    void Update(CompetitionEntry entry);
    void Delete(CompetitionEntry entry);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}



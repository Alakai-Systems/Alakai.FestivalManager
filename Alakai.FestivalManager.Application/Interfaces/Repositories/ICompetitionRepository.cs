namespace Alakai.FestivalManager.Application.Contracts.Repositories;

public interface ICompetitionRepository
{
    Task AddAsync(Competition competition, CancellationToken cancellationToken = default);
    Task<Competition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Competition>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Competition>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEditionAndNameAsync(Guid editionId, string name, CancellationToken cancellationToken = default);
    void Update(Competition competition);
    void Delete(Competition competition);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IUserPanelRepository
{
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Registration?> GetLatestRegistrationByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetRegistrationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompetitionEntry>> GetCompetitionEntriesByRegistrationIdsAsync(IReadOnlyList<Guid> registrationIds, CancellationToken cancellationToken = default);
}
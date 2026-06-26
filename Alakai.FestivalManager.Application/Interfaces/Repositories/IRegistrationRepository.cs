namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IRegistrationRepository
{
    Task AddAsync(Registration registration, CancellationToken cancellationToken = default);
    Task<Registration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Registration?> GetByUserIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Registration>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<Registration?> GetByEditionAndEmailAsync(Guid editionId, string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEditionAndEmailAsync(Guid editionId, string email, CancellationToken cancellationToken = default);
    Task<int> CountByDiscountCodeAsync(Guid discountCodeId, CancellationToken cancellationToken = default);
    Task<Registration?> GetByIdWithPartnerDataAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Registration?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<int> CountActiveByLevelAndRoleAsync(Guid levelId, DanceRole role, CancellationToken cancellationToken = default);
    void Update(Registration registration);
    void Delete(Registration registration);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

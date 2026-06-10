namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IPassTypeRepository
{
    Task<IReadOnlyList<PassType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PassType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PassType>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEditionAndNameAsync(Guid editionId, string name, CancellationToken cancellationToken = default);
    Task AddAsync(PassType passType, CancellationToken cancellationToken = default);
    void Update(PassType passType);
    void Delete(PassType passType);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

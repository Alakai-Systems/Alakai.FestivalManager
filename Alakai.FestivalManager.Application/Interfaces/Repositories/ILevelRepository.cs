namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface ILevelRepository
{
    Task<IReadOnlyList<Level>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Level?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Level>> GetByPassTypeIdAsync(Guid passTypeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPassTypeAndNameAsync(Guid passTypeId, string name, CancellationToken cancellationToken = default);
    Task AddAsync(Level level, CancellationToken cancellationToken = default);
    void Update(Level level);
    void Delete(Level level);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IEditionRepository
{
    Task<IReadOnlyList<Edition>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Edition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Edition>> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByFestivalAndYearAsync(Guid festivalId, int year, CancellationToken cancellationToken = default);
    Task AddAsync(Edition edition, CancellationToken cancellationToken = default);
    void Update(Edition edition);
    void Delete(Edition edition);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
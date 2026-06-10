namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IFestivalRepository
{
    Task<IReadOnlyList<Festival>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Festival?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Festival?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task AddAsync(Festival festival, CancellationToken cancellationToken = default);
    void Update(Festival festival);
    void Delete(Festival festival);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
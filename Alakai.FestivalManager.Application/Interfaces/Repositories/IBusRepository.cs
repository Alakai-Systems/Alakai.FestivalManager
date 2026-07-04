namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IBusRepository
{
    Task<IReadOnlyList<Bus>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<Bus?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Bus bus, CancellationToken cancellationToken = default);
    void Update(Bus bus);
    void Delete(Bus bus);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
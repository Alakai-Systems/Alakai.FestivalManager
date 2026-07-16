namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IFestivalCredentialsRepository
{
    Task<FestivalCredentials?> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default);
    Task AddAsync(FestivalCredentials credentials, CancellationToken cancellationToken = default);
    void Update(FestivalCredentials credentials);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
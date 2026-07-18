using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IMediaAssetRepository
{
    Task<IReadOnlyList<MediaAsset>> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default);
    Task AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default);
    Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Delete(MediaAsset mediaAsset);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
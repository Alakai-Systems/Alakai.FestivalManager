using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;
using Alakai.FestivalManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class MediaAssetRepository : IMediaAssetRepository
{
    private readonly FestivalManagerDbContext _context;

    public MediaAssetRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MediaAsset>> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default)
    {
        return await _context.MediaAssets
            .AsNoTracking()
            .Where(m => m.FestivalId == festivalId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
    {
        await _context.MediaAssets.AddAsync(mediaAsset, cancellationToken);
    }

    public async Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MediaAssets.FindAsync([id], cancellationToken);
    }

    public void Delete(MediaAsset mediaAsset)
    {
        _context.MediaAssets.Remove(mediaAsset);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
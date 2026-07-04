namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class AccommodationBuildingRepository : IAccommodationBuildingRepository
{
    private readonly FestivalManagerDbContext _context;

    public AccommodationBuildingRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AccommodationBuilding>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationBuildings
            .Include(b => b.Zones).ThenInclude(z => z.Accommodations)
            .Include(b => b.AllowedPassTypes).ThenInclude(p => p.PassType)
            .Where(b => b.EditionId == editionId)
            .OrderBy(b => b.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<AccommodationBuilding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationBuildings
            .Include(b => b.Zones.OrderBy(z => z.SortOrder)).ThenInclude(z => z.Accommodations.OrderBy(a => a.SortOrder))
            .Include(b => b.AllowedPassTypes).ThenInclude(p => p.PassType)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task AddAsync(AccommodationBuilding building, CancellationToken cancellationToken = default)
    {
        await _context.AccommodationBuildings.AddAsync(building, cancellationToken);
    }

    public void Update(AccommodationBuilding building)
    {
        _context.AccommodationBuildings.Update(building);
    }

    public void Delete(AccommodationBuilding building)
    {
        _context.AccommodationBuildings.Remove(building);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
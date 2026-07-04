namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class AccommodationZoneRepository : IAccommodationZoneRepository
{
    private readonly FestivalManagerDbContext _context;

    public AccommodationZoneRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<AccommodationZone?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationZones
            .Include(z => z.AccommodationBuilding)
            .FirstOrDefaultAsync(z => z.Id == id, cancellationToken);
    }

    public async Task AddAsync(AccommodationZone zone, CancellationToken cancellationToken = default)
    {
        await _context.AccommodationZones.AddAsync(zone, cancellationToken);
    }

    public void Update(AccommodationZone zone)
    {
        _context.AccommodationZones.Update(zone);
    }

    public void Delete(AccommodationZone zone)
    {
        _context.AccommodationZones.Remove(zone);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
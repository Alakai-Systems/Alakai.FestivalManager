namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class AccommodationRepository : IAccommodationRepository
{
    private readonly FestivalManagerDbContext _context;

    public AccommodationRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<Accommodation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accommodations
            .Include(a => a.AccommodationZone)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AddAsync(Accommodation accommodation, CancellationToken cancellationToken = default)
    {
        await _context.Accommodations.AddAsync(accommodation, cancellationToken);
    }

    public void Update(Accommodation accommodation)
    {
        _context.Accommodations.Update(accommodation);
    }

    public void Delete(Accommodation accommodation)
    {
        _context.Accommodations.Remove(accommodation);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
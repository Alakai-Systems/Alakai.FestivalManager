namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class BusRepository : IBusRepository
{
    private readonly FestivalManagerDbContext _context;

    public BusRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Bus>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.Buses
            .Include(b => b.AllowedPassTypes).ThenInclude(p => p.PassType)
            .Where(b => b.EditionId == editionId)
            .OrderBy(b => b.DepartureTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Bus?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Buses
            .Include(b => b.AllowedPassTypes).ThenInclude(p => p.PassType)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task AddAsync(Bus bus, CancellationToken cancellationToken = default)
    {
        await _context.Buses.AddAsync(bus, cancellationToken);
    }

    public void Update(Bus bus)
    {
        _context.Buses.Update(bus);
    }

    public void Delete(Bus bus)
    {
        _context.Buses.Remove(bus);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
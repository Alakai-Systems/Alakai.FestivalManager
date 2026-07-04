namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class BusReservationRepository : IBusReservationRepository
{
    private readonly FestivalManagerDbContext _context;

    public BusReservationRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<BusReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.BusReservations
            .Include(r => r.Bus)
            .Include(r => r.Registration)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<BusReservation>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.BusReservations
            .Include(r => r.Bus)
            .Where(r => r.RegistrationId == registrationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BusReservation>> GetByBusIdAsync(Guid busId, CancellationToken cancellationToken = default)
    {
        return await _context.BusReservations
            .Include(r => r.Registration)
            .Where(r => r.BusId == busId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BusReservation>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.BusReservations
            .Include(r => r.Bus)
            .Include(r => r.Registration)
            .Where(r => r.Bus.EditionId == editionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetOccupiedCountAsync(Guid busId, CancellationToken cancellationToken = default)
    {
        return await _context.BusReservations.CountAsync(r => r.BusId == busId, cancellationToken);
    }

    public async Task<bool> HasReservationForDirectionAsync(Guid registrationId, BusDirection direction, Guid? excludeReservationId, CancellationToken cancellationToken = default)
    {
        IQueryable<BusReservation> query = _context.BusReservations
            .Include(r => r.Bus)
            .Where(r => r.RegistrationId == registrationId && r.Bus.Direction == direction);

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(BusReservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.BusReservations.AddAsync(reservation, cancellationToken);
    }

    public void Delete(BusReservation reservation)
    {
        _context.BusReservations.Remove(reservation);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
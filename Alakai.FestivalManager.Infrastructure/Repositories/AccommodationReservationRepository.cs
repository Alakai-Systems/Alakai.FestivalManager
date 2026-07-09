namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class AccommodationReservationRepository : IAccommodationReservationRepository
{
    private readonly FestivalManagerDbContext _context;

    public AccommodationReservationRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<AccommodationReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationReservations
            .Include(r => r.Occupants).ThenInclude(o => o.Accommodation).ThenInclude(a => a!.AccommodationZone)
            .Include(r => r.ResponsibleRegistration)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AccommodationReservation>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationReservations
            .Include(r => r.Occupants).ThenInclude(o => o.Accommodation).ThenInclude(a => a!.AccommodationZone)
            .Include(r => r.Occupants).ThenInclude(o => o.Registration)
            .Include(r => r.ResponsibleRegistration)
            .Where(r => r.AccommodationBuildingId == buildingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<AccommodationReservation?> GetByResponsibleRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationReservations
            .Include(r => r.Occupants).ThenInclude(o => o.Accommodation).ThenInclude(a => a!.AccommodationZone)
            .Include(r => r.AccommodationBuilding)
            .FirstOrDefaultAsync(r => r.ResponsibleRegistrationId == registrationId, cancellationToken);
    }

    public async Task<AccommodationReservation?> GetByResponsibleRegistrationIdTrackedAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationReservations
            .Include(r => r.Occupants).ThenInclude(o => o.Registration)
            .FirstOrDefaultAsync(r => r.ResponsibleRegistrationId == registrationId, cancellationToken);
    }

    public async Task<AccommodationReservation?> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationReservations
            .AsNoTracking()
            .Include(r => r.Occupants).ThenInclude(o => o.Accommodation).ThenInclude(a => a!.AccommodationZone)
            .Include(r => r.Occupants).ThenInclude(o => o.Registration)
            .Include(r => r.AccommodationBuilding)
            .FirstOrDefaultAsync(r => r.ResponsibleRegistrationId == registrationId || r.Occupants.Any(o => o.RegistrationId == registrationId), cancellationToken);
    }

    public async Task<IReadOnlyList<AccommodationReservation>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationReservations
            .Include(r => r.Occupants).ThenInclude(o => o.Accommodation).ThenInclude(a => a!.AccommodationZone)
            .Include(r => r.Occupants).ThenInclude(o => o.Registration)
            .Include(r => r.AccommodationBuilding)
            .Where(r => r.EditionId == editionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsRegistrationAlreadyBookedAsync(Guid editionId, Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationReservationOccupants
            .Include(o => o.AccommodationReservation)
            .AnyAsync(o => o.RegistrationId == registrationId && o.AccommodationReservation.EditionId == editionId, cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetOccupancyCountsAsync(IReadOnlyList<Guid> accommodationIds, CancellationToken cancellationToken = default)
    {
        return await _context.AccommodationReservationOccupants
            .Where(o => o.AccommodationId != null && accommodationIds.Contains(o.AccommodationId.Value))
            .GroupBy(o => o.AccommodationId!.Value)
            .Select(g => new { AccommodationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AccommodationId, x => x.Count, cancellationToken);
    }

    public async Task AddAsync(AccommodationReservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.AccommodationReservations.AddAsync(reservation, cancellationToken);
    }

    public void Delete(AccommodationReservation reservation)
    {
        _context.AccommodationReservations.Remove(reservation);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
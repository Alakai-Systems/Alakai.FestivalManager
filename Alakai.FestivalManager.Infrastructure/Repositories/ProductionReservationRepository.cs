namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class ProductionReservationRepository : IProductionReservationRepository
{
    private readonly FestivalManagerDbContext _context;

    public ProductionReservationRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProductionAccommodationReservation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodationReservations
            .AsNoTracking()
            .Include(r => r.ProductionAccommodationBuilding)
            .Include(r => r.ResponsibleProductionPerson)
            .Include(r => r.Occupants).ThenInclude(o => o.ProductionPerson)
            .Include(r => r.Occupants).ThenInclude(o => o.ProductionAccommodation).ThenInclude(a => a!.ProductionAccommodationZone)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductionAccommodationReservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodationReservations
            .Include(r => r.ProductionAccommodationBuilding)
            .Include(r => r.ResponsibleProductionPerson)
            .Include(r => r.Occupants).ThenInclude(o => o.ProductionPerson)
            .Include(r => r.Occupants).ThenInclude(o => o.ProductionAccommodation).ThenInclude(a => a!.ProductionAccommodationZone)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProductionAccommodationReservation>> GetByBuildingIdAsync(Guid productionAccommodationBuildingId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductionAccommodationReservations
            .AsNoTracking()
            .Include(r => r.ProductionAccommodationBuilding)
            .Include(r => r.ResponsibleProductionPerson)
            .Include(r => r.Occupants).ThenInclude(o => o.ProductionPerson)
            .Include(r => r.Occupants).ThenInclude(o => o.ProductionAccommodation).ThenInclude(a => a!.ProductionAccommodationZone)
            .Where(r => r.ProductionAccommodationBuildingId == productionAccommodationBuildingId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ProductionAccommodationReservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.ProductionAccommodationReservations.AddAsync(reservation, cancellationToken);
    }

    public void Delete(ProductionAccommodationReservation reservation)
    {
        _context.ProductionAccommodationReservations.Remove(reservation);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
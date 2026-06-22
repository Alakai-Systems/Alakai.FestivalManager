namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class RegistrationRepository : IRegistrationRepository
{
    private readonly FestivalManagerDbContext _context;

    public RegistrationRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Registration registration, CancellationToken cancellationToken = default)
    {
        await _context.Registrations.AddAsync(registration, cancellationToken);
    }

    public async Task<Registration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Registrations
            .Include(r => r.PassType)
            .Include(r => r.Level)
            .Include(r => r.Edition)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Registration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Registrations
            .AsNoTracking()
            .Include(r => r.PassType)
            .Include(r => r.Level)
            .Include(r => r.Edition)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Registration>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.Registrations
            .AsNoTracking()
            .Where(r => r.EditionId == editionId)
            .Include(r => r.PassType)
            .Include(r => r.Level)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Registration?> GetByEditionAndEmailAsync(Guid editionId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Registrations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EditionId == editionId && r.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEditionAndEmailAsync(Guid editionId, string email, CancellationToken cancellationToken = default)
    {
        return await _context.Registrations.AnyAsync(r => r.EditionId == editionId && r.Email == email, cancellationToken);
    }

    public async Task<int> CountByDiscountCodeAsync(Guid discountCodeId, CancellationToken cancellationToken = default)
    {
        return await _context.Registrations.CountAsync(r => r.DiscountCodeId == discountCodeId, cancellationToken);
    }


    public void Update(Registration registration)
    {
        _context.Registrations.Update(registration);
    }

    public void Delete(Registration registration)
    {
        _context.Registrations.Remove(registration);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

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
            .Include(r => r.PartnerRegistration).ThenInclude(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Registration?> GetByUserIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Registrations
            .Include(r => r.PassType)
            .Include(r => r.Level)
            .Include(r => r.Edition)
            .Include(r => r.User)
            .Include(r => r.PartnerRegistration).ThenInclude(pr => pr.User)
            .Include(r => r.DiscountCode)
            .FirstOrDefaultAsync(r => r.UserId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Registration>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Registrations
            .AsNoTracking()
            .Include(r => r.PassType)
            .Include(r => r.Level)
            .Include(r => r.Edition)
            .Include(r => r.PartnerRegistration).ThenInclude(r => r.User)
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

    public async Task<Registration?> GetByIdWithPartnerDataAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Registrations
            .Include(r => r.PartnerRegistration)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, cancellationToken);
    }

    public async Task<Registration?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLower();

        return await _context.Registrations
            .FirstOrDefaultAsync(r => r.Email.ToLower() == normalizedEmail && r.IsActive, cancellationToken);
    }

    public async Task<int> CountActiveByLevelAndRoleAsync(Guid levelId, DanceRole role, CancellationToken cancellationToken = default)
    {
        return await _context.Registrations.CountAsync(r => r.LevelId == levelId && r.DanceRole == role && r.IsActive, cancellationToken);
    }
    public void Update(Registration registration)
    {
        _context.Registrations.Update(registration);
    }

    public void Delete(Registration registration)
    {
        _context.Registrations.Remove(registration);
    }

    public async Task<int> CountActiveByLevelAsync(Guid levelId, DanceRole? danceRole, CancellationToken cancellationToken = default)
    {
        IQueryable<Registration> query = _context.Registrations
            .Where(r => (r.LevelId == levelId || r.LevelSelections.Any(s => s.LevelId == levelId)) && r.IsActive && r.Status != RegistrationStatus.Cancelled);

        if (danceRole.HasValue)
        {
            query = query.Where(r => r.DanceRole == danceRole.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Registration?> GetByOrderAsync(string order, CancellationToken cancellationToken = default)
    {
        return await _context.Registrations
            .FirstOrDefaultAsync(r => r.PaymentReference == order, cancellationToken);
    }

    public async Task<Registration?> GetByPaymentReferenceAsync(string paymentReference, CancellationToken cancellationToken = default)
    {
        return await _context.Registrations
            .FirstOrDefaultAsync(r => r.PaymentReference == paymentReference, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

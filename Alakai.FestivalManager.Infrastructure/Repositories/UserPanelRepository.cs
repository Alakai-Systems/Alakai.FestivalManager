namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class UserPanelRepository : IUserPanelRepository
{
    private readonly FestivalManagerDbContext _context;

    public UserPanelRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<Registration?> GetLatestRegistrationByUserIdAsync(Guid userId, string? domain, CancellationToken cancellationToken = default)
    {
        IQueryable<Registration> baseQuery = _context.Registrations
            .Include(r => r.PassType)
            .Include(r => r.Level)
            .Include(r => r.Edition).ThenInclude(e => e.Festival)
            .Where(r => r.UserId == userId && r.IsActive);

        if (!string.IsNullOrWhiteSpace(domain))
        {
            Registration? scopedToFestival = await baseQuery
                .Where(r => r.Edition.Festival.CustomDomain == domain)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (scopedToFestival is not null)
            {
                return scopedToFestival;
            }
        }

        return await baseQuery
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Registration>> GetRegistrationsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Registrations
            .Where(r => r.UserId == userId && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompetitionEntry>> GetCompetitionEntriesByRegistrationIdsAsync(IReadOnlyList<Guid> registrationIds, CancellationToken cancellationToken = default)
    {
        return await _context.CompetitionEntries
            .Include(c => c.Competition)
            .Where(c => registrationIds.Contains(c.RegistrationId) && c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Invoice>> GetInvoicesByRegistrationIdsAsync(IReadOnlyList<Guid> registrationIds, CancellationToken cancellationToken = default)
    {
        return await _context.Invoices
            .Where(i => registrationIds.Contains(i.RegistrationId))
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
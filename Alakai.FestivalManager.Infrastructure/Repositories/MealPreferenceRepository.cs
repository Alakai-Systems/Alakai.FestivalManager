namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class MealPreferenceRepository : IMealPreferenceRepository
{
    private readonly FestivalManagerDbContext _context;

    public MealPreferenceRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<MealPreference?> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.MealPreferences.AsNoTracking().FirstOrDefaultAsync(m => m.RegistrationId == registrationId, cancellationToken);
    }

    public async Task<IReadOnlyList<MealPreference>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.MealPreferences
            .Include(m => m.Registration)
            .Where(m => m.Registration.EditionId == editionId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MealPreference preference, CancellationToken cancellationToken = default)
    {
        await _context.MealPreferences.AddAsync(preference, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
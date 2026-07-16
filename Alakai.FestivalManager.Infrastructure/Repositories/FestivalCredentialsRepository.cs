namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class FestivalCredentialsRepository : IFestivalCredentialsRepository
{
    private readonly FestivalManagerDbContext _context;

    public FestivalCredentialsRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<FestivalCredentials?> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default)
    {
        return await _context.FestivalCredentials
            .FirstOrDefaultAsync(fc => fc.FestivalId == festivalId, cancellationToken);
    }

    public async Task AddAsync(FestivalCredentials credentials, CancellationToken cancellationToken = default)
    {
        await _context.FestivalCredentials.AddAsync(credentials, cancellationToken);
    }

    public void Update(FestivalCredentials credentials)
    {
        _context.FestivalCredentials.Update(credentials);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
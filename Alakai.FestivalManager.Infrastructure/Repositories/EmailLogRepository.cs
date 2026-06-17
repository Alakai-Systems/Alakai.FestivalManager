namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class EmailLogRepository : IEmailLogRepository
{
    private readonly FestivalManagerDbContext _context;

    public EmailLogRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailLog emailLog, CancellationToken cancellationToken = default)
    {
        await _context.EmailLogs.AddAsync(emailLog, cancellationToken);
    }

    public async Task<EmailLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<EmailLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs.OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailLog>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs.Where(e => e.EditionId == editionId).OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailLog>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs.Where(e => e.RegistrationId == registrationId).OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailLogs.Where(e => e.UserId == userId).OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);
    }

    public void Update(EmailLog emailLog)
    {
        _context.EmailLogs.Update(emailLog);
    }

    public void Delete(EmailLog emailLog)
    {
        _context.EmailLogs.Remove(emailLog);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class EmailLayoutRepository : IEmailLayoutRepository
{
    private readonly FestivalManagerDbContext _context;

    public EmailLayoutRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task<EmailLayout?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailLayouts.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(EmailLayout emailLayout, CancellationToken cancellationToken = default)
    {
        await _context.EmailLayouts.AddAsync(emailLayout, cancellationToken);
    }

    public void Update(EmailLayout emailLayout)
    {
        _context.EmailLayouts.Update(emailLayout);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly FestivalManagerDbContext _context;

    public UserRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLower();
        return await _context.Users.FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.OrderBy(user => user.LastName).ThenBy(user => user.FirstName).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLower();
        return await _context.Users.AnyAsync(user => user.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task<bool> ExistsByEmailExceptIdAsync(string email, Guid id, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLower();
        return await _context.Users.AnyAsync(user => user.Id != id && user.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

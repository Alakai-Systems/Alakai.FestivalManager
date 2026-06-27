namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailExceptIdAsync(string email, Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    void Update(User user);
    void Delete(User user);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

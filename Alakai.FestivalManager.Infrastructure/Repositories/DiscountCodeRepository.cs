namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class DiscountCodeRepository : IDiscountCodeRepository
{
    private readonly FestivalManagerDbContext _context;

    public DiscountCodeRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DiscountCode discountCode, CancellationToken cancellationToken = default)
    {
        await _context.DiscountCodes.AddAsync(discountCode, cancellationToken);
    }

    public async Task<DiscountCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountCodes.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<DiscountCode?> GetByEditionAndCodeAsync(Guid editionId, string code, CancellationToken cancellationToken = default)
    {
        string normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.DiscountCodes.FirstOrDefaultAsync(d => d.EditionId == editionId && d.Code == normalizedCode, cancellationToken);
    }

    public async Task<IReadOnlyList<DiscountCode>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DiscountCodes.OrderBy(d => d.Code).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DiscountCode>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscountCodes.Where(d => d.EditionId == editionId).OrderBy(d => d.Code).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEditionAndCodeAsync(Guid editionId, string code, CancellationToken cancellationToken = default)
    {
        string normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.DiscountCodes.AnyAsync(d => d.EditionId == editionId && d.Code == normalizedCode, cancellationToken);
    }

    public void Update(DiscountCode discountCode)
    {
        _context.DiscountCodes.Update(discountCode);
    }

    public void Delete(DiscountCode discountCode)
    {
        _context.DiscountCodes.Remove(discountCode);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

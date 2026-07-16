namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class EmailTemplateRepository : IEmailTemplateRepository
{
    private readonly FestivalManagerDbContext _context;

    public EmailTemplateRepository(FestivalManagerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailTemplate emailTemplate, CancellationToken cancellationToken = default)
    {
        await _context.EmailTemplates.AddAsync(emailTemplate, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByTemplateKeyAsync(Guid? editionId, EmailTemplateKey templateKey, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EditionId == editionId && e.TemplateKey == templateKey, cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .AsNoTracking()
            .OrderBy(e => e.EditionId)
            .ThenBy(e => e.TemplateKey)
            .ThenBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .AsNoTracking()
            .Where(e => e.EditionId == editionId)
            .OrderBy(e => e.TemplateKey)
            .ThenBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEditionAndTemplateKeyAsync(Guid? editionId, EmailTemplateKey templateKey, string language, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _context.EmailTemplates
            .AnyAsync(e => e.EditionId == editionId && e.TemplateKey == templateKey && e.Language == language && (!excludeId.HasValue || e.Id != excludeId.Value), cancellationToken);
    }

    public async Task<EmailTemplate?> GetByKeyAsync(EmailTemplateKey templateKey, Guid? editionId, CancellationToken cancellationToken = default)
    {
        EmailTemplate? editionTemplate = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.TemplateKey == templateKey && t.EditionId == editionId && t.IsActive, cancellationToken);

        if (editionTemplate is not null)
        {
            return editionTemplate;
        }

        return await _context.EmailTemplates.FirstOrDefaultAsync(t => t.TemplateKey == templateKey && t.EditionId == null && t.IsActive, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByKeyAndLanguageAsync(EmailTemplateKey templateKey, Guid? editionId, string language, CancellationToken cancellationToken = default)
    {
        // 1. Edition-specific + exact language
        EmailTemplate? template = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.TemplateKey == templateKey && t.EditionId == editionId && t.Language == language && t.IsActive, cancellationToken);

        if (template is not null) return template;

        // 2. Edition-specific + fallback to English
        if (language != "en")
        {
            template = await _context.EmailTemplates
                .FirstOrDefaultAsync(t => t.TemplateKey == templateKey && t.EditionId == editionId && t.Language == "en" && t.IsActive, cancellationToken);
            if (template is not null) return template;
        }

        // 3. Global (no edition) + exact language
        template = await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.TemplateKey == templateKey && t.EditionId == null && t.Language == language && t.IsActive, cancellationToken);

        if (template is not null) return template;

        // 4. Global + fallback to English
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.TemplateKey == templateKey && t.EditionId == null && t.Language == "en" && t.IsActive, cancellationToken);
    }

    public void Update(EmailTemplate emailTemplate)
    {
        _context.EmailTemplates.Update(emailTemplate);
    }

    public void Delete(EmailTemplate emailTemplate)
    {
        _context.EmailTemplates.Remove(emailTemplate);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}


namespace Alakai.FestivalManager.Application.Features.Festivals.Commands.CreateFestival;

public class CreateFestivalHandler
{
    private readonly IApplicationDbContext _context;

    public CreateFestivalHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FestivalDto> HandleAsync(
        CreateFestivalCommand command,
        CancellationToken cancellationToken = default)
    {
        Festival festival = new()
        {
            Name = command.Name,
            Slug = command.Slug,
            Description = command.Description,
            Website = command.Website,
            LogoUrl = command.LogoUrl,
            IsActive = true
        };

        _context.Festivals.Add(festival);

        await _context.SaveChangesAsync(cancellationToken);

        return new FestivalDto
        {
            Id = festival.Id,
            Name = festival.Name,
            Slug = festival.Slug,
            Description = festival.Description,
            Website = festival.Website,
            LogoUrl = festival.LogoUrl,
            IsActive = festival.IsActive
        };
    }
}

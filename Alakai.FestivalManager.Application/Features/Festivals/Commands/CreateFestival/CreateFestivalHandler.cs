namespace Alakai.FestivalManager.Application.Features.Festivals.Commands.CreateFestival;

public class CreateFestivalHandler
{
    private readonly IFestivalRepository _festivalRepository;

    public CreateFestivalHandler(IFestivalRepository festivalRepository)
    {
        _festivalRepository = festivalRepository;
    }

    public async Task<FestivalDto> HandleAsync(
        CreateFestivalCommand command,
        CancellationToken cancellationToken = default)
    {
        bool slugExists = await _festivalRepository.ExistsBySlugAsync(
            command.Slug,
            cancellationToken);

        if (slugExists is true)
        {
            throw new InvalidOperationException(
                $"A festival with slug '{command.Slug}' already exists.");
        }

        var festival = new Festival
        {
            Name = command.Name,
            Slug = command.Slug,
            Description = command.Description,
            Website = command.Website,
            LogoUrl = command.LogoUrl,
            IsActive = true
        };

        await _festivalRepository.AddAsync(festival, cancellationToken);

        await _festivalRepository.SaveChangesAsync(cancellationToken);

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

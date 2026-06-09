namespace Alakai.FestivalManager.Application.Features.Festivals.Commands.CreateFestival;

public class CreateFestivalHandler
{
    private readonly IFestivalRepository _festivalRepository;
    private readonly IMapper _mapper;

    public CreateFestivalHandler(IFestivalRepository festivalRepository, IMapper mapper)
    {
        _festivalRepository = festivalRepository;
        _mapper = mapper;
    }

    public async Task<FestivalDto> HandleAsync(CreateFestivalCommand command, CancellationToken cancellationToken = default)
    {
        bool slugExists = await _festivalRepository.ExistsBySlugAsync(command.Slug, cancellationToken);

        if (slugExists is true)
        {
            throw new InvalidOperationException(
                $"A festival with slug '{command.Slug}' already exists.");
        }

        Festival festival = _mapper.Map<Festival>(command);

        await _festivalRepository.AddAsync(festival, cancellationToken);
        await _festivalRepository.SaveChangesAsync(cancellationToken);

        FestivalDto festivalDto = _mapper.Map<FestivalDto>(festival);

        return festivalDto;
    }
}

namespace Alakai.FestivalManager.Application.Features.Festivals.Commands.UpdateFestival;

public class UpdateFestivalHandler
{
    private readonly IFestivalRepository _festivalRepository;
    private readonly IMapper _mapper;

    public UpdateFestivalHandler(IFestivalRepository festivalRepository, IMapper mapper)
    {
        _festivalRepository = festivalRepository;
        _mapper = mapper;
    }

    public async Task<FestivalDto> HandleAsync(UpdateFestivalCommand command, CancellationToken cancellationToken = default)
    {
        Festival? festival = await _festivalRepository.GetByIdAsync(command.Id, cancellationToken);

        if (festival is null)
        {
            throw new NotFoundException(
                $"Festival with id '{command.Id}' was not found.");
        }

        _mapper.Map(command, festival);

        festival.SetUpdated();


        await _festivalRepository.SaveChangesAsync(cancellationToken);

        FestivalDto festivalDto = _mapper.Map<FestivalDto>(festival);

        return festivalDto;
    }
}
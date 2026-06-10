namespace Alakai.FestivalManager.Application.Features.Editions.Commands.UpdateEdition;

public class UpdateEditionHandler
{
    private readonly IEditionRepository _editionRepository;
    private readonly IFestivalRepository _festivalRepository;
    private readonly IMapper _mapper;

    public UpdateEditionHandler(IEditionRepository editionRepository, IFestivalRepository festivalRepository, IMapper mapper)
    {
        _editionRepository = editionRepository;
        _festivalRepository = festivalRepository;
        _mapper = mapper;
    }

    public async Task<EditionDto> HandleAsync(UpdateEditionCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.Id}' was not found.");
        }

        Festival? festival = await _festivalRepository.GetByIdAsync(command.FestivalId, cancellationToken);

        if (festival is null)
        {
            throw new NotFoundException($"Festival with id '{command.FestivalId}' was not found.");
        }

        _mapper.Map(command, edition);

        edition.SetUpdated();

        await _editionRepository.SaveChangesAsync(cancellationToken);

        EditionDto editionDto = _mapper.Map<EditionDto>(edition);

        return editionDto;
    }
}

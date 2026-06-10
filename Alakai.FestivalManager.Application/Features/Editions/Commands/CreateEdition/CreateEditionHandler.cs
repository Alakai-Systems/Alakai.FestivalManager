namespace Alakai.FestivalManager.Application.Features.Editions.Commands.CreateEdition;

public class CreateEditionHandler
{
    private readonly IEditionRepository _editionRepository;
    private readonly IFestivalRepository _festivalRepository;
    private readonly IMapper _mapper;

    public CreateEditionHandler(IEditionRepository editionRepository, IFestivalRepository festivalRepository, IMapper mapper)
    {
        _editionRepository = editionRepository;
        _festivalRepository = festivalRepository;
        _mapper = mapper;
    }

    public async Task<EditionDto> HandleAsync(CreateEditionCommand command, CancellationToken cancellationToken = default)
    {
        Festival? festival = await _festivalRepository.GetByIdAsync(command.FestivalId, cancellationToken);

        if (festival is null)
        {
            throw new NotFoundException($"Festival with id '{command.FestivalId}' was not found.");
        }

        bool exists = await _editionRepository.ExistsByFestivalAndYearAsync(command.FestivalId, command.Year, cancellationToken);

        if (exists is true)
        {
            throw new BusinessRuleException($"Edition for festival '{command.FestivalId}' and year '{command.Year}' already exists.");
        }

        Edition? edition = _mapper.Map<Edition>(command);

        await _editionRepository.AddAsync(edition, cancellationToken);
        await _editionRepository.SaveChangesAsync(cancellationToken);

        EditionDto editionDto = _mapper.Map<EditionDto>(edition);

        return editionDto;
    }
}
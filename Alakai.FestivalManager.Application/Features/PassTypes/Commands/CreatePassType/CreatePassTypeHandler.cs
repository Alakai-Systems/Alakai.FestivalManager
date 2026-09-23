namespace Alakai.FestivalManager.Application.Features.PassTypes.Commands.CreatePassType;

public class CreatePassTypeHandler
{
    private readonly IPassTypeRepository _passTypeRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IFestivalRepository _festivalRepository;
    private readonly IMapper _mapper;

    public CreatePassTypeHandler(IPassTypeRepository passTypeRepository, IEditionRepository editionRepository, IFestivalRepository festivalRepository, IMapper mapper)
    {
        _passTypeRepository = passTypeRepository;
        _editionRepository = editionRepository;
        _festivalRepository = festivalRepository;
        _mapper = mapper;
    }

    public async Task<PassTypeDto> HandleAsync(CreatePassTypeCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        bool exists = await _passTypeRepository.ExistsByEditionAndNameAsync(command.EditionId, command.Name, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"Pass type '{command.Name}' already exists for this edition.");
        }

        PassType passType = _mapper.Map<PassType>(command);

        // Un pase nuevo hereda los modulos que el festival tenga activos en este momento.
        // Competitions se fuerza siempre activo (independientemente del festival) para poder
        // desactivarlo despues, pase por pase, desde la pantalla de edicion.
        Festival? festival = await _festivalRepository.GetByIdAsync(edition.FestivalId, cancellationToken);
        passType.EnabledModules = (festival?.EnabledModules ?? FestivalModule.None) | FestivalModule.Competitions;

        await _passTypeRepository.AddAsync(passType, cancellationToken);
        await _passTypeRepository.SaveChangesAsync(cancellationToken);

        PassTypeDto passTypeDto = _mapper.Map<PassTypeDto>(passType);

        return passTypeDto;
    }
}
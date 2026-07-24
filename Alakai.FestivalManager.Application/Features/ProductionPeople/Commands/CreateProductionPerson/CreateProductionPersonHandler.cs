namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Commands.CreateProductionPerson;

public class CreateProductionPersonHandler
{
    private readonly IProductionPersonRepository _productionPersonRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreateProductionPersonHandler(IProductionPersonRepository productionPersonRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _productionPersonRepository = productionPersonRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<ProductionPersonDto> HandleAsync(CreateProductionPersonCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        bool exists = await _productionPersonRepository.ExistsByEditionAndDocumentNumberAsync(command.EditionId, command.DocumentNumber, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"A production person with document number '{command.DocumentNumber}' already exists for this edition.");
        }

        ProductionPerson productionPerson = _mapper.Map<ProductionPerson>(command);

        await _productionPersonRepository.AddAsync(productionPerson, cancellationToken);
        await _productionPersonRepository.SaveChangesAsync(cancellationToken);

        ProductionPersonDto productionPersonDto = _mapper.Map<ProductionPersonDto>(productionPerson);

        return productionPersonDto;
    }
}
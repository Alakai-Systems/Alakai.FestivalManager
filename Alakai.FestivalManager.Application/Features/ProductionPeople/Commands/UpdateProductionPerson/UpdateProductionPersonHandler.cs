namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Commands.UpdateProductionPerson;

public class UpdateProductionPersonHandler
{
    private readonly IProductionPersonRepository _productionPersonRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdateProductionPersonHandler(IProductionPersonRepository productionPersonRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _productionPersonRepository = productionPersonRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<ProductionPersonDto> HandleAsync(UpdateProductionPersonCommand command, CancellationToken cancellationToken = default)
    {
        ProductionPerson? productionPerson = await _productionPersonRepository.GetByIdAsync(command.Id, cancellationToken);

        if (productionPerson is null)
        {
            throw new NotFoundException($"Production person with id '{command.Id}' was not found.");
        }

        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        _mapper.Map(command, productionPerson);
        productionPerson.SetUpdated();

        await _productionPersonRepository.SaveChangesAsync(cancellationToken);

        ProductionPersonDto productionPersonDto = _mapper.Map<ProductionPersonDto>(productionPerson);

        return productionPersonDto;
    }
}
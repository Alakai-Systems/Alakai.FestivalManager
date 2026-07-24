namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Commands.DeleteProductionPerson;

public class DeleteProductionPersonHandler
{
    private readonly IProductionPersonRepository _productionPersonRepository;

    public DeleteProductionPersonHandler(IProductionPersonRepository productionPersonRepository)
    {
        _productionPersonRepository = productionPersonRepository;
    }

    public async Task<Guid> HandleAsync(DeleteProductionPersonCommand command, CancellationToken cancellationToken = default)
    {
        ProductionPerson? productionPerson = await _productionPersonRepository.GetByIdAsync(command.Id, cancellationToken);

        if (productionPerson is null)
        {
            throw new NotFoundException($"Production person with id '{command.Id}' was not found.");
        }

        _productionPersonRepository.Delete(productionPerson);

        await _productionPersonRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}
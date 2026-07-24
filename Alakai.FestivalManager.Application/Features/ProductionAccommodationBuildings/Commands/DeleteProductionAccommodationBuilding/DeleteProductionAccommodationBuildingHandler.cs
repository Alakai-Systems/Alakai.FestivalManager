namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Commands.DeleteProductionAccommodationBuilding;

public class DeleteProductionAccommodationBuildingHandler
{
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;

    public DeleteProductionAccommodationBuildingHandler(IProductionAccommodationBuildingRepository buildingRepository)
    {
        _buildingRepository = buildingRepository;
    }

    public async Task<Guid> HandleAsync(DeleteProductionAccommodationBuildingCommand command, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationBuilding? building = await _buildingRepository.GetByIdAsync(command.Id, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException($"Production accommodation building with id '{command.Id}' was not found.");
        }

        _buildingRepository.Delete(building);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}
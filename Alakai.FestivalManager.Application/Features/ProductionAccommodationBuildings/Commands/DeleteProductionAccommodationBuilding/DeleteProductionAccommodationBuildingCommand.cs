namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Commands.DeleteProductionAccommodationBuilding;

public class DeleteProductionAccommodationBuildingCommand
{
    public Guid Id { get; set; }
    public DeleteProductionAccommodationBuildingCommand(Guid id)
    {
        Id = id;
    }
}
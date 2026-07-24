namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Commands.DeleteBuilding;

public class DeleteBuildingCommand
{
    public Guid Id { get; set; }
    public DeleteBuildingCommand(Guid id)
    {
        Id = id;
    }
}
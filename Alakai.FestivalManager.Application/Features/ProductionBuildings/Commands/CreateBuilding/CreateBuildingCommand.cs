namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Commands.CreateBuilding;

public class CreateBuildingCommand
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccommodationType Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
}
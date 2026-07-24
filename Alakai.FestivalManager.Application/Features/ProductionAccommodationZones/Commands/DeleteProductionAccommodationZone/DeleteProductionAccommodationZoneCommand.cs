namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Commands.DeleteProductionAccommodationZone;

public class DeleteProductionAccommodationZoneCommand
{
    public Guid Id { get; set; }
    public DeleteProductionAccommodationZoneCommand(Guid id)
    {
        Id = id;
    }
}
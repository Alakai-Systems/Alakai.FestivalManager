namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Commands.DeleteProductionAccommodation;

public class DeleteProductionAccommodationCommand
{
    public Guid Id { get; set; }
    public DeleteProductionAccommodationCommand(Guid id)
    {
        Id = id;
    }
}
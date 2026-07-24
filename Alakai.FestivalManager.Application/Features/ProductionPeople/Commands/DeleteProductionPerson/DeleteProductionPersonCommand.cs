namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Commands.DeleteProductionPerson;

public class DeleteProductionPersonCommand
{
    public Guid Id { get; set; }
    public DeleteProductionPersonCommand(Guid id)
    {
        Id = id;
    }
}
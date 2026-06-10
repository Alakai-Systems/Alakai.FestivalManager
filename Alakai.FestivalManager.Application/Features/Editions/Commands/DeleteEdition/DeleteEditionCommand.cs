namespace Alakai.FestivalManager.Application.Features.Editions.Commands.DeleteEdition;

public class DeleteEditionCommand
{
    public Guid Id { get; set; }
    public DeleteEditionCommand(Guid id)
    {
        Id = id;
    }
}
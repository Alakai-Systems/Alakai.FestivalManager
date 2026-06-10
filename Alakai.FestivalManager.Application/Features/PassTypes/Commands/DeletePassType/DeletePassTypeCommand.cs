namespace Alakai.FestivalManager.Application.Features.PassTypes.Commands.DeletePassType;

public class DeletePassTypeCommand
{
    public Guid Id { get; set; }
    public DeletePassTypeCommand(Guid id)
    {
        Id = id;
    }
}

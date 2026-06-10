namespace Alakai.FestivalManager.Application.Features.Festivals.Commands.DeleteFestival;

public class DeleteFestivalCommand
{
    public Guid Id { get; set; }
    public DeleteFestivalCommand(Guid id)
    {
        Id = id;
    }
}

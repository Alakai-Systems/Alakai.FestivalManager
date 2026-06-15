namespace Alakai.FestivalManager.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommand
{
    public Guid Id { get; set; }

    public DeleteUserCommand(Guid id)
    {
        Id = id;
    }
}

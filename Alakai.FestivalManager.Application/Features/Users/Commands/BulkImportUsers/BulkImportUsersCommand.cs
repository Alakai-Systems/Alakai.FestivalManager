namespace Alakai.FestivalManager.Application.Features.Users.Commands.BulkImportUsers;

public class BulkImportUsersCommand
{
    public List<CreateUserCommand> Rows { get; set; } = [];
}
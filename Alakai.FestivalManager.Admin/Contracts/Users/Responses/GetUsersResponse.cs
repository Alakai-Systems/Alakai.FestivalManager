namespace Alakai.FestivalManager.Admin.Contracts.Users.Responses;

public class GetUsersResponse
{
    public IReadOnlyList<UserDto> Users { get; set; } = [];
}

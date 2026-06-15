namespace Alakai.FestivalManager.Application.Features.Users.Contracts.Responses;

public class DeleteUserResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}

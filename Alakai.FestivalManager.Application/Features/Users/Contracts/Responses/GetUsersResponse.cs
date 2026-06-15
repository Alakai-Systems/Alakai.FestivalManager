using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Users.Contracts.Responses;

public class GetUsersResponse
{
    public IReadOnlyList<UserDto> Users { get; set; } = [];
}

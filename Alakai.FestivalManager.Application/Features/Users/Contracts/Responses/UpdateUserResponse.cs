using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Users.Contracts.Responses;

public class UpdateUserResponse
{
    public UserDto User { get; set; } = default!;
}

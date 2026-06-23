namespace Alakai.FestivalManager.Application.Features.Auth.Contracts.Responses;

public class GetCurrentUserResponse
{
    public AuthUserDto User { get; set; } = default!;
}

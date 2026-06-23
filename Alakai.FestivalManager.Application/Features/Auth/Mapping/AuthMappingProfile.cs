namespace Alakai.FestivalManager.Application.Features.Auth.Mapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<LoginRequest, LoginCommand>();
        CreateMap<RefreshTokenRequest, RefreshTokenCommand>();
        CreateMap<ForgotPasswordRequest, ForgotPasswordCommand>();
        CreateMap<ResetPasswordRequest, ResetPasswordCommand>();
        CreateMap<ChangePasswordRequest, ChangePasswordCommand>();
        CreateMap<User, AuthUserDto>();
    }
}

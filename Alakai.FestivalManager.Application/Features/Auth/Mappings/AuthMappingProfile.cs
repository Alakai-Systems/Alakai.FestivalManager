namespace Alakai.FestivalManager.Application.Features.Auth.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<LoginRequest, LoginCommand>();
        CreateMap<ExternalLoginRequest, ExternalLoginCommand>();
        CreateMap<RefreshTokenRequest, RefreshTokenCommand>();
        CreateMap<ForgotPasswordRequest, ForgotPasswordCommand>();
        CreateMap<ResetPasswordRequest, ResetPasswordCommand>();
        CreateMap<ChangePasswordRequest, ChangePasswordCommand>();
        CreateMap<User, AuthUserDto>();
    }
}

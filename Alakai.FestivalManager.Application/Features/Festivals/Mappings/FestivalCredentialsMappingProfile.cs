namespace Alakai.FestivalManager.Application.Features.Festivals.Mappings;

public class FestivalCredentialsMappingProfile : Profile
{
    public FestivalCredentialsMappingProfile()
    {
        CreateMap<FestivalCredentials, FestivalCredentialsDto>()
            .ForMember(dest => dest.HasRedsysSecretKey, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.RedsysSecretKey)))
            .ForMember(dest => dest.HasEmailPassword, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.EmailPassword)));

        CreateMap<UpsertFestivalCredentialsRequest, UpsertFestivalCredentialsCommand>();
    }
}
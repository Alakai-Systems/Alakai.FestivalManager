namespace Alakai.FestivalManager.Application.Features.EmailLogs.Mappings;

public class EmailLogMappingProfile : Profile
{
    public EmailLogMappingProfile()
    {
        //Generics y Gets
        CreateMap<EmailLog, EmailLogDto>();
        CreateMap<IReadOnlyList<EmailLogDto>, IReadOnlyList<EmailLog>>();

        //Create
        CreateMap<CreateEmailLogCommand, EmailLog>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.EmailTemplate, opt => opt.Ignore())
            .ForMember(dest => dest.Registration, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());
        CreateMap<CreateEmailLogRequest, CreateEmailLogCommand>();
        CreateMap<EmailLogDto, CreateEmailLogResponse>();

        //Update
        CreateMap<UpdateEmailLogCommand, EmailLog>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.EmailTemplate, opt => opt.Ignore())
            .ForMember(dest => dest.Registration, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
        CreateMap<UpdateEmailLogRequest, UpdateEmailLogCommand>();
        CreateMap<EmailLogDto, UpdateEmailLogResponse>();
    }
}
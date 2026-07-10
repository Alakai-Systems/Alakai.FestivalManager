namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Mappings;

public class EmailTemplateMappingProfile : Profile
{
    public EmailTemplateMappingProfile()
    {
        //Generics and Gets
        CreateMap<EmailTemplate, EmailTemplateDto>();
        CreateMap<IReadOnlyList<EmailTemplateDto>, IReadOnlyList<EmailTemplate>>();

        //Create
        CreateMap<CreateEmailTemplateRequest, CreateEmailTemplateCommand>();
        CreateMap<CreateEmailTemplateCommand, EmailTemplate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<EmailTemplateDto, CreateEmailTemplateResponse>();

        //Update
        CreateMap<UpdateEmailTemplateRequest, UpdateEmailTemplateCommand>();
        CreateMap<UpdateEmailTemplateCommand, EmailTemplate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<EmailTemplateDto, UpdateEmailTemplateResponse>();
    }
}

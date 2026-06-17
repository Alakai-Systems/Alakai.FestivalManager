namespace Alakai.FestivalManager.Application.Features.Registrations.Mapping;

public class RegistrationMappingProfile : Profile
{
    public RegistrationMappingProfile()
    {
        //Generics y Gets
        CreateMap<Registration, RegistrationDto>();
        CreateMap<IReadOnlyList<RegistrationDto>, IReadOnlyList<Registration>>();

        //Create Registration
        CreateMap<CreateRegistrationCommand, Registration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.PassType, opt => opt.Ignore())
            .ForMember(dest => dest.Level, opt => opt.Ignore())
            .ForMember(dest => dest.PartnerRegistration, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.DiscountCode, opt => opt.Ignore())
            .ForMember(dest => dest.DiscountCodeId, opt => opt.Ignore())
            .ForMember(dest => dest.DiscountCodeValue, opt => opt.Ignore())
            .ForMember(dest => dest.DiscountStatus, opt => opt.Ignore());
        CreateMap<CreateRegistrationRequest, CreateRegistrationCommand>();
        CreateMap<RegistrationDto, CreateRegistrationResponse>();

        //Update Registration
        CreateMap<UpdateRegistrationCommand, Registration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.PassType, opt => opt.Ignore())
            .ForMember(dest => dest.Level, opt => opt.Ignore())
            .ForMember(dest => dest.PartnerRegistration, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.DiscountCode, opt => opt.Ignore())
            .ForMember(dest => dest.DiscountCodeId, opt => opt.Ignore())
            .ForMember(dest => dest.DiscountCodeValue, opt => opt.Ignore())
            .ForMember(dest => dest.DiscountStatus, opt => opt.Ignore());
        CreateMap<UpdateRegistrationRequest, UpdateRegistrationCommand>();
        CreateMap<RegistrationDto, UpdateRegistrationResponse>();
    }
}
namespace Alakai.FestivalManager.Application.Features.Registrations.Mapping;

public class RegistrationMappingProfile : Profile
{
    public RegistrationMappingProfile()
    {
        //Generics y Gets
        CreateMap<Registration, RegistrationDto>()
            .ForMember(dest => dest.DiscountCodeValue, opt => opt.MapFrom(src => src.DiscountCodeValue))
            .ForMember(dest => dest.DiscountAmount, opt => opt.MapFrom(src => src.DiscountAmount))
            .ForMember(dest => dest.FinalPrice, opt => opt.MapFrom(src => src.FinalPrice))
            .ForMember(dest => dest.DiscountStatus, opt => opt.MapFrom(src => src.DiscountStatus))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(s => s.User.FirstName + " " + s.User.LastName))
            .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(s =>
                s.PartnerRegistration == null
                    ? null
                    : s.PartnerRegistration.User.FirstName + " " + s.PartnerRegistration.User.LastName));
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
            .ForMember(dest => dest.DiscountCode, opt => opt.Ignore());
        CreateMap<CreateRegistrationRequest, CreateRegistrationCommand>()
           .ForMember(dest => dest.PaymentPlan, opt => opt.MapFrom(src => src.PaymentPlan))
           .ForMember(dest => dest.ManagementFee, opt => opt.MapFrom(src => src.ManagementFee))
           .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid))
           .ForMember(dest => dest.LevelIds, opt => opt.MapFrom(src => src.LevelIds));
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
            .ForMember(dest => dest.DiscountStatus, opt => opt.Ignore());
        CreateMap<UpdateRegistrationRequest, UpdateRegistrationCommand>();
        CreateMap<RegistrationDto, UpdateRegistrationResponse>();
    }
}
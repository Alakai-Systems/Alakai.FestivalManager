namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Mappings;

public class DiscountCodeMappingProfile : Profile
{
    public DiscountCodeMappingProfile()
    {
        CreateMap<DiscountCode, DiscountCodeDto>();

        CreateMap<CreateDiscountCodeRequest, CreateDiscountCodeCommand>();
        CreateMap<UpdateDiscountCodeRequest, UpdateDiscountCodeCommand>();

        CreateMap<CreateDiscountCodeCommand, DiscountCode>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentUses, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        CreateMap<UpdateDiscountCodeCommand, DiscountCode>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore());
    }
}

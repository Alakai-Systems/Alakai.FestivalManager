namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Mappings;

public class CompetitionEntryMappingProfile : Profile
{
    public CompetitionEntryMappingProfile()
    {
        CreateMap<CompetitionEntry, CompetitionEntryDto>();

        CreateMap<CreateCompetitionEntryCommand, CompetitionEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Competition, opt => opt.Ignore())
            .ForMember(dest => dest.Registration, opt => opt.Ignore())
            .ForMember(dest => dest.PartnerRegistration, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());

        CreateMap<UpdateCompetitionEntryCommand, CompetitionEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Competition, opt => opt.Ignore())
            .ForMember(dest => dest.Registration, opt => opt.Ignore())
            .ForMember(dest => dest.PartnerRegistration, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}

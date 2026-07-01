using Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.CompetitionEntries.Contracts.Requests;

namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Mappings;

public class CompetitionEntryMappingProfile : Profile
{
    public CompetitionEntryMappingProfile()
    {
        //Generics y Gets
        CreateMap<CompetitionEntry, CompetitionEntryDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Registration != null ? src.Registration.FirstName : null))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Registration != null ? src.Registration.LastName : null))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Registration != null ? src.Registration.Email : null));
        CreateMap<IReadOnlyList<CompetitionEntryDto>, IReadOnlyList<CompetitionEntry>>();

        //Create
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
        CreateMap<CreateCompetitionEntryRequest, CreateCompetitionEntryCommand>();
        CreateMap<CompetitionEntryDto, CreateCompetitionEntryResponse>();


        CreateMap<UpdateCompetitionEntryCommand, CompetitionEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Competition, opt => opt.Ignore())
            .ForMember(dest => dest.Registration, opt => opt.Ignore())
            .ForMember(dest => dest.PartnerRegistration, opt => opt.Ignore())
            .ForMember(dest => dest.CompetitionCapacity, opt => opt.Ignore());
        CreateMap<UpdateCompetitionEntryRequest, UpdateCompetitionEntryCommand>();
        CreateMap<CompetitionEntryDto, UpdateCompetitionEntryResponse>();
    }
}

namespace Alakai.FestivalManager.Application.Features.Competitions.Mappings;

public class CompetitionMappingProfile : Profile
{
    public CompetitionMappingProfile()
    {
        //Generic and gets
        CreateMap<CompetitionLevel, CompetitionLevelDto>();
        CreateMap<CompetitionCapacity, CompetitionCapacityDto>();
        CreateMap<Competition, CompetitionDto>();
        CreateMap<IReadOnlyList<CompetitionDto>, IReadOnlyList<Competition>>();

        //Create
        CreateMap<CreateCompetitionCommand, Competition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Levels, opt => opt.Ignore())
            .ForMember(dest => dest.Capacities, opt => opt.Ignore());

        CreateMap<CreateCompetitionRequest, CreateCompetitionCommand>();

        //Update
        CreateMap<UpdateCompetitionCommand, Competition>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Edition, opt => opt.Ignore())
            .ForMember(dest => dest.Entries, opt => opt.Ignore())
            .ForMember(dest => dest.Levels, opt => opt.Ignore())
            .ForMember(dest => dest.Capacities, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<UpdateCompetitionRequest, UpdateCompetitionCommand>();
        CreateMap<CompetitionDto, CreateCompetitionResponse>();

        //Capacity
        CreateMap<UpdateCompetitionCapacityCommand, CompetitionCapacity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Competition, opt => opt.Ignore())
            .ForMember(dest => dest.CompetitionLevel, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        CreateMap<CompetitionDto, UpdateCompetitionResponse>();
    }
}

namespace Alakai.FestivalManager.Application.Features.Levels.Mappings;

public class LevelMappingProfile : Profile
{
    public LevelMappingProfile()
    {
        //Generics and Gets
        CreateMap<Level, LevelDto>();
        CreateMap<IReadOnlyList<LevelDto>, IReadOnlyList<Level>>();

        //Create Level
        CreateMap<CreateLevelRequest, CreateLevelCommand>();
        CreateMap<CreateLevelCommand, Level>();
        CreateMap<LevelDto, CreateLevelResponse>();

        //Update Level
        CreateMap<UpdateLevelRequest, UpdateLevelCommand>();
        CreateMap<UpdateLevelCommand, Level>();
        CreateMap<LevelDto, UpdateLevelResponse>();
    }
}
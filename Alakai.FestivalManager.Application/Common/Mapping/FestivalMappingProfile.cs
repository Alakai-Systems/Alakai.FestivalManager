using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Common.Mappings;

public class FestivalMappingProfile : Profile
{
    public FestivalMappingProfile()
    {
        CreateMap<CreateFestivalCommand, Festival>();

        CreateMap<Festival, FestivalDto>();

        CreateMap<FestivalDto, CreateFestivalResponse>();

        CreateMap<IReadOnlyList<FestivalDto>, IReadOnlyList<Festival>>();
    }
}
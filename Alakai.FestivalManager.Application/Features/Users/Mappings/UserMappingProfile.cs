using Alakai.FestivalManager.Application.Features.Users.Commands.CreateUser;
using Alakai.FestivalManager.Application.Features.Users.Commands.UpdateUser;
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;
using Alakai.FestivalManager.Domain.Entities;
using AutoMapper;

namespace Alakai.FestivalManager.Application.Features.Users.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<CreateUserCommand, User>();
        CreateMap<UpdateUserCommand, User>();
        CreateMap<User, UserDto>();
    }
}

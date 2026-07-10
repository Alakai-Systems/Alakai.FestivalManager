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
        CreateMap<CreateUserCommand, User>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.PhotoUrl, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.Role, o => o.Ignore())
            .ForMember(d => d.LastLoginAt, o => o.Ignore())
            .ForMember(d => d.PasswordResetToken, o => o.Ignore())
            .ForMember(d => d.PasswordResetTokenExpiresAt, o => o.Ignore())
            .ForMember(d => d.PasswordChangedAt, o => o.Ignore())
            .ForMember(d => d.MustChangePassword, o => o.Ignore())
            .ForMember(d => d.IsLocked, o => o.Ignore())
            .ForMember(d => d.FailedLoginAttempts, o => o.Ignore())
            .ForMember(d => d.LockoutEndAt, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.Ignore())
            .ForMember(d => d.Registrations, o => o.Ignore());

        CreateMap<UpdateUserCommand, User>()
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.LastLoginAt, o => o.Ignore())
            .ForMember(d => d.PasswordResetToken, o => o.Ignore())
            .ForMember(d => d.PasswordResetTokenExpiresAt, o => o.Ignore())
            .ForMember(d => d.PasswordChangedAt, o => o.Ignore())
            .ForMember(d => d.MustChangePassword, o => o.Ignore())
            .ForMember(d => d.IsLocked, o => o.Ignore())
            .ForMember(d => d.FailedLoginAttempts, o => o.Ignore())
            .ForMember(d => d.LockoutEndAt, o => o.Ignore())
            .ForMember(d => d.Registrations, o => o.Ignore());

        CreateMap<User, UserDto>();
    }
}
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Auth.Services;
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;
using Alakai.FestivalManager.Domain.Enums;
using AutoMapper;

namespace Alakai.FestivalManager.Application.Features.Users.Commands.CreateAdminUser;

public class CreateAdminUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;

    public CreateAdminUserHandler(IUserRepository userRepository, IPasswordService passwordService, IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _mapper = mapper;
    }

    public async Task<UserDto> HandleAsync(CreateAdminUserCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Role != UserRole.Admin && command.Role != UserRole.SuperAdmin)
        {
            throw new BusinessRuleException("Role must be Admin or SuperAdmin.");
        }

        string normalizedEmail = command.Email.Trim().ToLowerInvariant();
        bool exists = await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"A user with email '{normalizedEmail}' already exists.");
        }

        User user = new()
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = normalizedEmail,
            Role = command.Role,
            IsActive = true,
            PasswordChangedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordService.HashPassword(user, command.Password);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;
using Alakai.FestivalManager.Domain.Entities;
using AutoMapper;

namespace Alakai.FestivalManager.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UpdateUserHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User with id '{command.Id}' was not found.");
        }

        bool exists = await _userRepository.ExistsByEmailExceptIdAsync(command.Email, command.Id, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"A user with email '{command.Email}' already exists.");
        }

        _mapper.Map<User>(command);

        user.SetUpdated();
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}

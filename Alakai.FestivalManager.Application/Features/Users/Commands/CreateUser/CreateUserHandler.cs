namespace Alakai.FestivalManager.Application.Features.Users.Commands.CreateUser;

public class CreateUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public CreateUserHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        bool exists = await _userRepository.ExistsByEmailAsync(command.Email, cancellationToken);

        if (exists)
        {
            throw new BusinessRuleException($"A user with email '{command.Email}' already exists.");
        }

        User user = _mapper.Map<User>(command);
        user.Email = command.Email.Trim().ToLower();
        user.IsActive = true;

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}

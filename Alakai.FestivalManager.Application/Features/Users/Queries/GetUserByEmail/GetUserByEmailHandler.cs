namespace Alakai.FestivalManager.Application.Features.Users.Queries.GetUserByEmail;

public class GetUserByEmailHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserByEmailHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto> HandleAsync(GetUserByEmailQuery query, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.GetByEmailAsync(query.Email, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User with email '{query.Email}' was not found.");
        }

        return _mapper.Map<UserDto>(user);
    }
}

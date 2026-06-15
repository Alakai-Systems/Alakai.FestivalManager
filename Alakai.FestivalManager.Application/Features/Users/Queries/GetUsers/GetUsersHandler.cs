using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;
using AutoMapper;

namespace Alakai.FestivalManager.Application.Features.Users.Queries.GetUsers;

public class GetUsersHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUsersHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<UserDto>> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        return _mapper.Map<IReadOnlyList<UserDto>>(await _userRepository.GetAllAsync(cancellationToken));
    }
}

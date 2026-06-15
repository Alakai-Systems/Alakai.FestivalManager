using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;
using Alakai.FestivalManager.Domain.Entities;
using AutoMapper;

namespace Alakai.FestivalManager.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetUserByIdHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.GetByIdAsync(query.Id, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User with id '{query.Id}' was not found.");
        }

        return _mapper.Map<UserDto>(user);
    }
}

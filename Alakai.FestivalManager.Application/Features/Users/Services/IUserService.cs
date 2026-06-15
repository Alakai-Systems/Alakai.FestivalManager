using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Users.Commands.CreateUser;
using Alakai.FestivalManager.Application.Features.Users.Commands.UpdateUser;
using Alakai.FestivalManager.Application.Features.Users.Contracts.Responses;

namespace Alakai.FestivalManager.Application.Features.Users.Services;

public interface IUserService
{
    Task<ApiResponse<CreateUserResponse>> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUsersResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserByIdResponse>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateUserResponse>> UpdateAsync(Guid id, UpdateUserCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteUserResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

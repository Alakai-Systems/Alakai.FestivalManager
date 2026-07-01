using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Users.Commands.CreateAdminUser;
using Alakai.FestivalManager.Application.Features.Users.Commands.CreateUser;
using Alakai.FestivalManager.Application.Features.Users.Commands.DeleteUser;
using Alakai.FestivalManager.Application.Features.Users.Commands.UpdateUser;
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.Users.Contracts.Responses;
using Alakai.FestivalManager.Application.Features.Users.Queries.GetUserByEmail;
using Alakai.FestivalManager.Application.Features.Users.Queries.GetUserById;
using Alakai.FestivalManager.Application.Features.Users.Queries.GetUsers;
using FluentValidation;

namespace Alakai.FestivalManager.Application.Features.Users.Services;

public class UserService : IUserService
{
    private readonly CreateUserHandler _createUserHandler;
    private readonly CreateAdminUserHandler _createAdminUserHandler;
    private readonly UpdateUserHandler _updateUserHandler;
    private readonly DeleteUserHandler _deleteUserHandler;
    private readonly GetUserByIdHandler _getUserByIdHandler;
    private readonly GetUsersHandler _getUsersHandler;
    private readonly GetUserByEmailHandler _getUserByEmailHandler;
    private readonly IValidator<CreateUserCommand> _createUserValidator;
    private readonly IValidator<UpdateUserCommand> _updateUserValidator;
    private readonly IValidator<CreateAdminUserCommand> _createAdminUserValidator;

    public UserService(CreateUserHandler createUserHandler, CreateAdminUserHandler createAdminUserHandler, UpdateUserHandler updateUserHandler, DeleteUserHandler deleteUserHandler, GetUserByIdHandler getUserByIdHandler, GetUsersHandler getUsersHandler, GetUserByEmailHandler getUserByEmailHandler, IValidator<CreateUserCommand> createUserValidator, IValidator<UpdateUserCommand> updateUserValidator, IValidator<CreateAdminUserCommand> createAdminUserValidator)
    {
        _createUserHandler = createUserHandler;
        _createAdminUserHandler = createAdminUserHandler;
        _updateUserHandler = updateUserHandler;
        _deleteUserHandler = deleteUserHandler;
        _getUserByIdHandler = getUserByIdHandler;
        _getUsersHandler = getUsersHandler;
        _getUserByEmailHandler = getUserByEmailHandler;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
        _createAdminUserValidator = createAdminUserValidator;
    }

    public async Task<ApiResponse<CreateUserResponse>> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        await _createUserValidator.ValidateAndThrowAsync(command, cancellationToken);
        UserDto userDto = await _createUserHandler.HandleAsync(command, cancellationToken);
        return new ApiResponse<CreateUserResponse> { Success = true, Data = new CreateUserResponse { User = userDto }, Errors = [], Message = $"{userDto.Email} is correctly registered" };
    }

    public async Task<ApiResponse<CreateUserResponse>> CreateAdminAsync(CreateAdminUserCommand command, CancellationToken cancellationToken = default)
    {
        await _createAdminUserValidator.ValidateAndThrowAsync(command, cancellationToken);
        UserDto userDto = await _createAdminUserHandler.HandleAsync(command, cancellationToken);
        return new ApiResponse<CreateUserResponse> { Success = true, Data = new CreateUserResponse { User = userDto }, Errors = [], Message = $"{userDto.Email} was created as {userDto.Role}" };
    }

    public async Task<ApiResponse<GetUserByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        UserDto userDto = await _getUserByIdHandler.HandleAsync(new GetUserByIdQuery(id), cancellationToken);
        return new ApiResponse<GetUserByIdResponse> { Success = true, Data = new GetUserByIdResponse { User = userDto }, Errors = [], Message = "User found" };
    }

    public async Task<ApiResponse<GetUsersResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<UserDto> userDtos = await _getUsersHandler.HandleAsync(new GetUsersQuery(), cancellationToken);
        return new ApiResponse<GetUsersResponse> { Success = true, Data = new GetUsersResponse { Users = userDtos }, Errors = [], Message = $"There are {userDtos.Count} users registered" };
    }

    public async Task<ApiResponse<GetUserByIdResponse>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        UserDto userDto = await _getUserByEmailHandler.HandleAsync(new GetUserByEmailQuery(email), cancellationToken);
        return new ApiResponse<GetUserByIdResponse> { Success = true, Data = new GetUserByIdResponse { User = userDto }, Errors = [], Message = "User found" };
    }

    public async Task<ApiResponse<UpdateUserResponse>> UpdateAsync(Guid id, UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;
        await _updateUserValidator.ValidateAndThrowAsync(command, cancellationToken);
        UserDto userDto = await _updateUserHandler.HandleAsync(command, cancellationToken);
        return new ApiResponse<UpdateUserResponse> { Success = true, Data = new UpdateUserResponse { User = userDto }, Errors = [], Message = $"{userDto.Email} is correctly updated" };
    }

    public async Task<ApiResponse<DeleteUserResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        bool deleted = await _deleteUserHandler.HandleAsync(new DeleteUserCommand(id), cancellationToken);
        return new ApiResponse<DeleteUserResponse> { Success = true, Data = new DeleteUserResponse { Id = id, Deleted = deleted }, Errors = [], Message = "User deleted" };
    }
}
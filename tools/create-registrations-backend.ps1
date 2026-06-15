param(
    [string]$Root = (Get-Location).Path
)

$ErrorActionPreference = "Stop"

function Write-File {
    param(
        [string]$Path,
        [string]$Content
    )

    $FullPath = Join-Path $Root $Path
    $Directory = Split-Path $FullPath -Parent

    if (!(Test-Path $Directory)) {
        New-Item -ItemType Directory -Path $Directory -Force | Out-Null
    }

    Set-Content -Path $FullPath -Value $Content -Encoding UTF8
    Write-Host "Created/updated $Path"
}

Write-Host "Creating Users backend structure in $Root"

Write-File "Alakai.FestivalManager.Domain\Entities\User.cs" @'
namespace Alakai.FestivalManager.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Registration> Registrations { get; set; } = [];
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Contracts\DTOs\UserDto.cs" @'
namespace Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Commands\CreateUser\CreateUserCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommand
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Commands\UpdateUser\UpdateUserCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Commands\DeleteUser\DeleteUserCommand.cs" @'
namespace Alakai.FestivalManager.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommand
{
    public Guid Id { get; set; }

    public DeleteUserCommand(Guid id)
    {
        Id = id;
    }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Queries\GetUserById\GetUserByIdQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQuery
{
    public Guid Id { get; set; }

    public GetUserByIdQuery(Guid id)
    {
        Id = id;
    }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Queries\GetUsers\GetUsersQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.Users.Queries.GetUsers;

public class GetUsersQuery
{
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Queries\GetUserByEmail\GetUserByEmailQuery.cs" @'
namespace Alakai.FestivalManager.Application.Features.Users.Queries.GetUserByEmail;

public class GetUserByEmailQuery
{
    public string Email { get; set; }

    public GetUserByEmailQuery(string email)
    {
        Email = email;
    }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Contracts\Responses\CreateUserResponse.cs" @'
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Users.Contracts.Responses;

public class CreateUserResponse
{
    public UserDto User { get; set; } = default!;
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Contracts\Responses\UpdateUserResponse.cs" @'
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Users.Contracts.Responses;

public class UpdateUserResponse
{
    public UserDto User { get; set; } = default!;
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Contracts\Responses\DeleteUserResponse.cs" @'
namespace Alakai.FestivalManager.Application.Features.Users.Contracts.Responses;

public class DeleteUserResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Contracts\Responses\GetUserByIdResponse.cs" @'
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Users.Contracts.Responses;

public class GetUserByIdResponse
{
    public UserDto User { get; set; } = default!;
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Contracts\Responses\GetUsersResponse.cs" @'
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Users.Contracts.Responses;

public class GetUsersResponse
{
    public IReadOnlyList<UserDto> Users { get; set; } = [];
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Mappings\UserMappingProfile.cs" @'
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
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Repositories\IUserRepository.cs" @'
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Users.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailExceptIdAsync(string email, Guid id, CancellationToken cancellationToken = default);
    void Update(User user);
    void Delete(User user);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
'@

Write-File "Alakai.FestivalManager.Infrastructure\Repositories\UserRepository.cs" @'
using Alakai.FestivalManager.Application.Features.Users.Repositories;
using Alakai.FestivalManager.Domain.Entities;
using Alakai.FestivalManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Alakai.FestivalManager.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLower();
        return await _context.Users.FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users.OrderBy(user => user.LastName).ThenBy(user => user.FirstName).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLower();
        return await _context.Users.AnyAsync(user => user.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task<bool> ExistsByEmailExceptIdAsync(string email, Guid id, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLower();
        return await _context.Users.AnyAsync(user => user.Id != id && user.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Commands\CreateUser\CreateUserHandler.cs" @'
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Users.Commands.CreateUser;
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.Users.Repositories;
using Alakai.FestivalManager.Domain.Entities;
using AutoMapper;

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
        user.Id = Guid.NewGuid();
        user.Email = command.Email.Trim().ToLower();
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Commands\UpdateUser\UpdateUserHandler.cs" @'
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.Users.Repositories;
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

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.Email = command.Email.Trim().ToLower();
        user.Phone = command.Phone;
        user.Country = command.Country;
        user.City = command.City;
        user.IsActive = command.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Commands\DeleteUser\DeleteUserHandler.cs" @'
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Users.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserHandler
{
    private readonly IUserRepository _userRepository;

    public DeleteUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException($"User with id '{command.Id}' was not found.");
        }

        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Queries\GetUsers\GetUsersHandler.cs" @'
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.Users.Repositories;
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
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Queries\GetUserById\GetUserByIdHandler.cs" @'
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.Users.Repositories;
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
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Queries\GetUserByEmail\GetUserByEmailHandler.cs" @'
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Users.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.Users.Repositories;
using Alakai.FestivalManager.Domain.Entities;
using AutoMapper;

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
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Services\IUserService.cs" @'
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
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Services\UserService.cs" @'
using Alakai.FestivalManager.Application.Common.Responses;
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
    private readonly UpdateUserHandler _updateUserHandler;
    private readonly DeleteUserHandler _deleteUserHandler;
    private readonly GetUserByIdHandler _getUserByIdHandler;
    private readonly GetUsersHandler _getUsersHandler;
    private readonly GetUserByEmailHandler _getUserByEmailHandler;
    private readonly IValidator<CreateUserCommand> _createUserValidator;
    private readonly IValidator<UpdateUserCommand> _updateUserValidator;

    public UserService(CreateUserHandler createUserHandler, UpdateUserHandler updateUserHandler, DeleteUserHandler deleteUserHandler, GetUserByIdHandler getUserByIdHandler, GetUsersHandler getUsersHandler, GetUserByEmailHandler getUserByEmailHandler, IValidator<CreateUserCommand> createUserValidator, IValidator<UpdateUserCommand> updateUserValidator)
    {
        _createUserHandler = createUserHandler;
        _updateUserHandler = updateUserHandler;
        _deleteUserHandler = deleteUserHandler;
        _getUserByIdHandler = getUserByIdHandler;
        _getUsersHandler = getUsersHandler;
        _getUserByEmailHandler = getUserByEmailHandler;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
    }

    public async Task<ApiResponse<CreateUserResponse>> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        await _createUserValidator.ValidateAndThrowAsync(command, cancellationToken);
        UserDto userDto = await _createUserHandler.HandleAsync(command, cancellationToken);
        return new ApiResponse<CreateUserResponse> { Success = true, Data = new CreateUserResponse { User = userDto }, Errors = [], Message = $"{userDto.Email} is correctly registered" };
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
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Validators\CreateUserCommandValidator.cs" @'
using Alakai.FestivalManager.Application.Features.Users.Commands.CreateUser;
using FluentValidation;

namespace Alakai.FestivalManager.Application.Features.Users.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(command => command.Phone).MaximumLength(50);
        RuleFor(command => command.Country).MaximumLength(100);
        RuleFor(command => command.City).MaximumLength(100);
    }
}
'@

Write-File "Alakai.FestivalManager.Application\Features\Users\Validators\UpdateUserCommandValidator.cs" @'
using Alakai.FestivalManager.Application.Features.Users.Commands.UpdateUser;
using FluentValidation;

namespace Alakai.FestivalManager.Application.Features.Users.Validators;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(command => command.Phone).MaximumLength(50);
        RuleFor(command => command.Country).MaximumLength(100);
        RuleFor(command => command.City).MaximumLength(100);
    }
}
'@

Write-File "Alakai.FestivalManager.Api\Controllers\UsersController.cs" @'
using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Users.Commands.CreateUser;
using Alakai.FestivalManager.Application.Features.Users.Commands.UpdateUser;
using Alakai.FestivalManager.Application.Features.Users.Contracts.Responses;
using Alakai.FestivalManager.Application.Features.Users.Services;
using Microsoft.AspNetCore.Mvc;

namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetUsersResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GetUserByIdResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetByIdAsync(id, cancellationToken));
    }

    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<ApiResponse<GetUserByIdResponse>>> GetByEmail(string email, CancellationToken cancellationToken)
    {
        return Ok(await _userService.GetByEmailAsync(email, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateUserResponse>>> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _userService.CreateAsync(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UpdateUserResponse>>> Update(Guid id, [FromBody] UpdateUserCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _userService.UpdateAsync(id, command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<DeleteUserResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _userService.DeleteAsync(id, cancellationToken));
    }
}
'@

Write-Host ""
Write-Host "Users backend files created. Manual steps still required:" -ForegroundColor Yellow
Write-Host "1. Add DbSet<User> Users to ApplicationDbContext." -ForegroundColor Yellow
Write-Host "2. Configure User entity in OnModelCreating: required fields, max lengths, unique Email index, relationship with Registration." -ForegroundColor Yellow
Write-Host "3. Add UserId to Registration entity if not already done." -ForegroundColor Yellow
Write-Host "4. Register Users handlers/service in ApplicationDependencyInjectionExtension." -ForegroundColor Yellow
Write-Host "5. Register IUserRepository/UserRepository in InfrastructureDependencyInjectionExtension." -ForegroundColor Yellow
Write-Host "6. Create EF migration: dotnet ef migrations add AddUsers -p Alakai.FestivalManager.Infrastructure -s Alakai.FestivalManager.Api" -ForegroundColor Yellow
Write-Host "7. Update database: dotnet ef database update -p Alakai.FestivalManager.Infrastructure -s Alakai.FestivalManager.Api" -ForegroundColor Yellow

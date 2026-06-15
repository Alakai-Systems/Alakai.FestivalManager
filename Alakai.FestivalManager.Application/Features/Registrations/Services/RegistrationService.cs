namespace Alakai.FestivalManager.Application.Features.Registrations.Services;

public partial class RegistrationService : IRegistrationService
{
    private readonly CreateRegistrationHandler _createRegistrationHandler;
    private readonly GetRegistrationByIdHandler _getRegistrationByIdHandler;
    private readonly GetRegistrationsHandler _getRegistrationsHandler;
    private readonly GetRegistrationsByEditionIdHandler _getRegistrationsByEditionIdHandler;
    private readonly UpdateRegistrationHandler _updateRegistrationHandler;
    private readonly DeleteRegistrationHandler _deleteRegistrationHandler;
    private readonly IValidator<CreateRegistrationCommand> _createRegistrationValidator;
    private readonly IValidator<UpdateRegistrationCommand> _updateRegistrationValidator;

    public RegistrationService(CreateRegistrationHandler createRegistrationHandler, GetRegistrationByIdHandler getRegistrationByIdHandler, GetRegistrationsHandler getRegistrationsHandler, 
        GetRegistrationsByEditionIdHandler getRegistrationsByEditionIdHandler, UpdateRegistrationHandler updateRegistrationHandler, DeleteRegistrationHandler deleteRegistrationHandler, 
        IValidator<CreateRegistrationCommand> createRegistrationValidator, IValidator<UpdateRegistrationCommand> updateRegistrationValidator)
    {
        _createRegistrationHandler = createRegistrationHandler;
        _getRegistrationByIdHandler = getRegistrationByIdHandler;
        _getRegistrationsHandler = getRegistrationsHandler;
        _getRegistrationsByEditionIdHandler = getRegistrationsByEditionIdHandler;
        _updateRegistrationHandler = updateRegistrationHandler;
        _deleteRegistrationHandler = deleteRegistrationHandler;
        _createRegistrationValidator = createRegistrationValidator;
        _updateRegistrationValidator = updateRegistrationValidator;
    }

    public async Task<ApiResponse<CreateRegistrationResponse>> CreateAsync(CreateRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createRegistrationValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        RegistrationDto regDto = await _createRegistrationHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreateRegistrationResponse>
        {
            Success = true,
            Message = "Registration created successfully.",
            Data = new CreateRegistrationResponse { Registration = regDto },
            Errors = new List<string>()
        };
    }

    public async Task<ApiResponse<GetRegistrationByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RegistrationDto? dto = await _getRegistrationByIdHandler.HandleAsync(new GetRegistrationByIdQuery(id), cancellationToken);

        if (dto is null)
        {
            throw new NotFoundException($"Registration with id '{id}' was not found.");
        }

        return new ApiResponse<GetRegistrationByIdResponse>
        {
            Success = true,
            Message = string.Empty,
            Data = new GetRegistrationByIdResponse { Registration = dto },
            Errors = new List<string>()
        };
    }

    public async Task<ApiResponse<GetRegistrationsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RegistrationDto> dtos = await _getRegistrationsHandler.HandleAsync(new GetRegistrationsQuery(), cancellationToken);

        return new ApiResponse<GetRegistrationsResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} registrations registered.",
            Data = new GetRegistrationsResponse { Registrations = dtos },
            Errors = new List<string>()
        };
    }

    public async Task<ApiResponse<GetRegistrationsByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<RegistrationDto> dtos = await _getRegistrationsByEditionIdHandler.HandleAsync(new GetRegistrationsByEditionIdQuery(editionId), cancellationToken);

        return new ApiResponse<GetRegistrationsByEditionIdResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} registrations for this edition.",
            Data = new GetRegistrationsByEditionIdResponse { Registrations = dtos },
            Errors = new List<string>()
        };
    }

    public async Task<ApiResponse<UpdateRegistrationResponse>> UpdateAsync(Guid id, UpdateRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        FluentValidation.Results.ValidationResult validationResult = await _updateRegistrationValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        RegistrationDto dto = await _updateRegistrationHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateRegistrationResponse>
        {
            Success = true,
            Message = "Registration updated successfully.",
            Data = new UpdateRegistrationResponse { Registration = dto },
            Errors = new List<string>()
        };
    }

    public async Task<ApiResponse<DeleteRegistrationResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteRegistrationCommand command = new DeleteRegistrationCommand(id);
        Guid deletedId = await _deleteRegistrationHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteRegistrationResponse>
        {
            Success = true,
            Message = "Registration deleted successfully.",
            Data = new DeleteRegistrationResponse { Id = deletedId, Deleted = true },
            Errors = new List<string>()
        };
    }
}

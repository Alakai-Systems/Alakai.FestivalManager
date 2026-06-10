namespace Alakai.FestivalManager.Application.Features.PassTypes.Services;

public class PassTypeService : IPassTypeService
{
    private readonly CreatePassTypeHandler _createPassTypeHandler;
    private readonly GetPassTypeByIdHandler _getPassTypeByIdHandler;
    private readonly GetPassTypesHandler _getPassTypesHandler;
    private readonly GetPassTypesByEditionIdHandler _getPassTypesByEditionIdHandler;
    private readonly UpdatePassTypeHandler _updatePassTypeHandler;
    private readonly DeletePassTypeHandler _deletePassTypeHandler;
    private readonly IValidator<CreatePassTypeCommand> _createPassTypeValidator;
    private readonly IValidator<UpdatePassTypeCommand> _updatePassTypeValidator;

    public PassTypeService(CreatePassTypeHandler createPassTypeHandler, GetPassTypeByIdHandler getPassTypeByIdHandler, GetPassTypesHandler getPassTypesHandler, GetPassTypesByEditionIdHandler getPassTypesByEditionIdHandler, UpdatePassTypeHandler updatePassTypeHandler, DeletePassTypeHandler deletePassTypeHandler, IValidator<CreatePassTypeCommand> createPassTypeValidator, IValidator<UpdatePassTypeCommand> updatePassTypeValidator)
    {
        _createPassTypeHandler = createPassTypeHandler;
        _getPassTypeByIdHandler = getPassTypeByIdHandler;
        _getPassTypesHandler = getPassTypesHandler;
        _getPassTypesByEditionIdHandler = getPassTypesByEditionIdHandler;
        _updatePassTypeHandler = updatePassTypeHandler;
        _deletePassTypeHandler = deletePassTypeHandler;
        _createPassTypeValidator = createPassTypeValidator;
        _updatePassTypeValidator = updatePassTypeValidator;
    }

    public async Task<ApiResponse<CreatePassTypeResponse>> CreateAsync(CreatePassTypeCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createPassTypeValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        PassTypeDto passTypeDto = await _createPassTypeHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreatePassTypeResponse>
        {
            Success = true,
            Message = "Pass type created successfully.",
            Data = new CreatePassTypeResponse { PassType = passTypeDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetPassTypeByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetPassTypeByIdQuery query = new(id);

        PassTypeDto? passTypeDto = await _getPassTypeByIdHandler.HandleAsync(query, cancellationToken);

        if (passTypeDto is null)
        {
            throw new NotFoundException($"Pass type with id '{id}' was not found.");
        }

        return new ApiResponse<GetPassTypeByIdResponse>
        {
            Success = true,
            Message = $"Pass type with id '{id}' was found.",
            Data = new GetPassTypeByIdResponse { PassType = passTypeDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetPassTypesResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        GetPassTypesQuery query = new();

        IReadOnlyList<PassTypeDto> passTypeDtos = await _getPassTypesHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetPassTypesResponse>
        {
            Success = true,
            Message = $"There are {passTypeDtos.Count} PassTypes registered.",
            Data = new GetPassTypesResponse { PassTypes = passTypeDtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetPassTypesResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        GetPassTypesByEditionIdQuery query = new(editionId);

        IReadOnlyList<PassTypeDto> passTypeDtos = await _getPassTypesByEditionIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetPassTypesResponse>
        {
            Success = true,
            Message = $"There are {passTypeDtos.Count} PassTypes registered at this edition.",
            Data = new GetPassTypesResponse { PassTypes = passTypeDtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdatePassTypeResponse>> UpdateAsync(UpdatePassTypeCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updatePassTypeValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        PassTypeDto passTypeDto = await _updatePassTypeHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdatePassTypeResponse>
        {
            Success = true,
            Message = "Pass type updated successfully.",
            Data = new UpdatePassTypeResponse { PassType = passTypeDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeletePassTypeResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeletePassTypeCommand command = new(id);

        Guid deletedId = await _deletePassTypeHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeletePassTypeResponse>
        {
            Success = true,
            Message = "Pass type deleted successfully.",
            Data = new DeletePassTypeResponse { Id = deletedId, Deleted = true },
            Errors = []
        };
    }
}
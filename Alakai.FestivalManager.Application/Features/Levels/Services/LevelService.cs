namespace Alakai.FestivalManager.Application.Features.Levels.Services;

public class LevelService : ILevelService
{
    private readonly CreateLevelHandler _createLevelHandler;
    private readonly GetLevelByIdHandler _getLevelByIdHandler;
    private readonly GetLevelsHandler _getLevelsHandler;
    private readonly GetLevelsByPassTypeIdHandler _getLevelsByPassTypeIdHandler;
    private readonly UpdateLevelHandler _updateLevelHandler;
    private readonly DeleteLevelHandler _deleteLevelHandler;
    private readonly IValidator<CreateLevelCommand> _createLevelValidator;
    private readonly IValidator<UpdateLevelCommand> _updateLevelValidator;

    public LevelService(CreateLevelHandler createLevelHandler, GetLevelByIdHandler getLevelByIdHandler, GetLevelsHandler getLevelsHandler, GetLevelsByPassTypeIdHandler getLevelsByPassTypeIdHandler, UpdateLevelHandler updateLevelHandler, DeleteLevelHandler deleteLevelHandler, IValidator<CreateLevelCommand> createLevelValidator, IValidator<UpdateLevelCommand> updateLevelValidator)
    {
        _createLevelHandler = createLevelHandler;
        _getLevelByIdHandler = getLevelByIdHandler;
        _getLevelsHandler = getLevelsHandler;
        _getLevelsByPassTypeIdHandler = getLevelsByPassTypeIdHandler;
        _updateLevelHandler = updateLevelHandler;
        _deleteLevelHandler = deleteLevelHandler;
        _createLevelValidator = createLevelValidator;
        _updateLevelValidator = updateLevelValidator;
    }

    public async Task<ApiResponse<CreateLevelResponse>> CreateAsync(CreateLevelCommand command, CancellationToken cancellationToken = default)
    {
        FluentValidation.Results.ValidationResult validationResult = await _createLevelValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        LevelDto levelDto = await _createLevelHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreateLevelResponse>
        {
            Success = true,
            Message = "Level created successfully.",
            Data = new CreateLevelResponse { Level = levelDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetLevelByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetLevelByIdQuery query = new(id);

        LevelDto? levelDto = await _getLevelByIdHandler.HandleAsync(query, cancellationToken);

        if (levelDto is null)
        {
            throw new NotFoundException($"Level with id '{id}' was not found.");
        }

        return new ApiResponse<GetLevelByIdResponse>
        {
            Success = true,
            Message = string.Empty,
            Data = new GetLevelByIdResponse { Level = levelDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetLevelsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        GetLevelsQuery query = new();

        IReadOnlyList<LevelDto> levelDtos = await _getLevelsHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetLevelsResponse>
        {
            Success = true,
            Message = $"There are {levelDtos.Count} levels registered.",
            Data = new GetLevelsResponse { Levels = levelDtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetLevelsResponse>> GetByPassTypeIdAsync(Guid passTypeId, CancellationToken cancellationToken = default)
    {
        GetLevelsByPassTypeIdQuery query = new(passTypeId);

        IReadOnlyList<LevelDto> levelDtos = await _getLevelsByPassTypeIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetLevelsResponse>
        {
            Success = true,
            Message = $"There are {levelDtos.Count} levels registered for this type of pass.",
            Data = new GetLevelsResponse { Levels = levelDtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateLevelResponse>> UpdateAsync(UpdateLevelCommand command, CancellationToken cancellationToken = default)
    {
        FluentValidation.Results.ValidationResult validationResult = await _updateLevelValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        LevelDto levelDto = await _updateLevelHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateLevelResponse>
        {
            Success = true,
            Message = "Level updated successfully.",
            Data = new UpdateLevelResponse { Level = levelDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteLevelResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteLevelCommand command = new(id);

        Guid deletedId = await _deleteLevelHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteLevelResponse>
        {
            Success = true,
            Message = "Level deleted successfully.",
            Data = new DeleteLevelResponse { Id = deletedId, Deleted = true },
            Errors = []
        };
    }
}
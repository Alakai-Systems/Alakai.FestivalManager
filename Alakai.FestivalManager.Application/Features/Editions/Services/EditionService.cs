using Alakai.FestivalManager.Application.Features.Editions.Queries.GetEditionsByFestivalId;

namespace Alakai.FestivalManager.Application.Features.Editions.Services;

public class EditionService : IEditionService
{
    private readonly CreateEditionHandler _createEditionHandler;
    private readonly GetEditionByIdHandler _getEditionByIdHandler;
    private readonly GetEditionsByFestivalIdHandler _getEditionsByFestivalIdHandler;
    private readonly GetEditionsHandler _getEditionsHandler;
    private readonly UpdateEditionHandler _updateEditionHandler;
    private readonly DeleteEditionHandler _deleteEditionHandler;
    private readonly IValidator<CreateEditionCommand> _createEditionValidator;
    private readonly IValidator<UpdateEditionCommand> _updateEditionValidator;

    public EditionService(CreateEditionHandler createEditionHandler, GetEditionByIdHandler getEditionByIdHandler, GetEditionsByFestivalIdHandler getEditionsByFestivalIdHandler, 
        GetEditionsHandler getEditionsHandler, UpdateEditionHandler updateEditionHandler, DeleteEditionHandler deleteEditionHandler, 
        IValidator<CreateEditionCommand> createEditionValidator, IValidator<UpdateEditionCommand> updateEditionValidator)
    {
        _createEditionHandler = createEditionHandler;
        _getEditionByIdHandler = getEditionByIdHandler;
        _getEditionsHandler = getEditionsHandler;
        _updateEditionHandler = updateEditionHandler;
        _deleteEditionHandler = deleteEditionHandler;
        _createEditionValidator = createEditionValidator;
        _updateEditionValidator = updateEditionValidator;
        _getEditionsByFestivalIdHandler = getEditionsByFestivalIdHandler;
    }

    public async Task<ApiResponse<CreateEditionResponse>> CreateAsync(CreateEditionCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createEditionValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        EditionDto editionDto = await _createEditionHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreateEditionResponse>
        {
            Success = true,
            Message = "Edition created successfully.",
            Data = new CreateEditionResponse { Edition = editionDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetEditionByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetEditionByIdQuery query = new(id);

        EditionDto? editionDto = await _getEditionByIdHandler.HandleAsync(query, cancellationToken);

        if (editionDto is null)
        {
            throw new NotFoundException($"Edition with id '{id}' was not found.");
        }

        return new ApiResponse<GetEditionByIdResponse>
        {
            Success = true,
            Message = string.Empty,
            Data = new GetEditionByIdResponse { Edition = editionDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetEditionsResponse>> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default)
    {
        GetEditionsByFestivalIdQuery query = new(festivalId);

        IReadOnlyList<EditionDto> editionDtos = await _getEditionsByFestivalIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetEditionsResponse>
        {
            Success = true,
            Message = $"There are {editionDtos.Count} editions registered for this festival",
            Data = new GetEditionsResponse { Editions = editionDtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetEditionsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        GetEditionsQuery query = new();

        IReadOnlyList<EditionDto> editionDtos = await _getEditionsHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetEditionsResponse>
        {
            Success = true,
            Message = $"There are {editionDtos.Count} editions registered",
            Data = new GetEditionsResponse { Editions = editionDtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateEditionResponse>> UpdateAsync(UpdateEditionCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateEditionValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        EditionDto editionDto = await _updateEditionHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateEditionResponse>
        {
            Success = true,
            Message = "Edition updated successfully.",
            Data = new UpdateEditionResponse { Edition = editionDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteEditionResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteEditionCommand command = new(id);

        Guid deletedId = await _deleteEditionHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteEditionResponse>
        {
            Success = true,
            Message = "Edition deleted successfully.",
            Data = new DeleteEditionResponse { Id = deletedId, Deleted = true },
            Errors = []
        };
    }
}

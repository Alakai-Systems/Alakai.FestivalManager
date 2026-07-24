namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Services;

public class RunnerItineraryService : IRunnerItineraryService
{
    private readonly CreateItineraryHandler _createHandler;
    private readonly GetItineraryByIdHandler _getByIdHandler;
    private readonly GetItinerariesByEditionIdHandler _getByEditionIdHandler;
    private readonly UpdateItineraryHandler _updateHandler;
    private readonly DeleteItineraryHandler _deleteHandler;
    private readonly IValidator<CreateItineraryCommand> _createValidator;
    private readonly IValidator<UpdateItineraryCommand> _updateValidator;

    public RunnerItineraryService(CreateItineraryHandler createHandler, GetItineraryByIdHandler getByIdHandler, GetItinerariesByEditionIdHandler getByEditionIdHandler, UpdateItineraryHandler updateHandler, DeleteItineraryHandler deleteHandler, IValidator<CreateItineraryCommand> createValidator, IValidator<UpdateItineraryCommand> updateValidator)
    {
        _createHandler = createHandler;
        _getByIdHandler = getByIdHandler;
        _getByEditionIdHandler = getByEditionIdHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<CreateItineraryResponse>> CreateAsync(CreateItineraryCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        RunnerItineraryDto dto = await _createHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreateItineraryResponse>
        {
            Success = true,
            Message = "Itinerary created successfully.",
            Data = new CreateItineraryResponse { Itinerary = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetItineraryResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetItineraryByIdQuery query = new(id);
        RunnerItineraryDto? dto = await _getByIdHandler.HandleAsync(query, cancellationToken);

        if (dto is null) { throw new NotFoundException($"Itinerary with id '{id}' was not found."); }

        return new ApiResponse<GetItineraryResponse>
        {
            Success = true,
            Message = $"Itinerary with id '{id}' was found.",
            Data = new GetItineraryResponse { Itinerary = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetItinerariesResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        GetItinerariesByEditionIdQuery query = new(editionId);
        IReadOnlyList<RunnerItineraryDto> dtos = await _getByEditionIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetItinerariesResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} itineraries registered for this edition.",
            Data = new GetItinerariesResponse { Itineraries = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateItineraryResponse>> UpdateAsync(UpdateItineraryCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        RunnerItineraryDto dto = await _updateHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateItineraryResponse>
        {
            Success = true,
            Message = "Itinerary updated successfully.",
            Data = new UpdateItineraryResponse { Itinerary = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteItineraryResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteItineraryCommand command = new(id);
        Guid deletedId = await _deleteHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteItineraryResponse>
        {
            Success = true,
            Message = "Itinerary deleted successfully.",
            Data = new DeleteItineraryResponse { Id = deletedId, Deleted = true },
            Errors = []
        };
    }
}
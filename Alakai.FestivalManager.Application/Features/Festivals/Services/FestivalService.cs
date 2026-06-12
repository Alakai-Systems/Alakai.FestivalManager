namespace Alakai.FestivalManager.Application.Features.Festivals.Services;

public class FestivalService : IFestivalService
{
    private readonly CreateFestivalHandler _createFestivalHandler;
    private readonly GetFestivalByIdHandler _getFestivalByIdHandler;
    private readonly GetFestivalsHandler _getFestivalsHandler;
    private readonly UpdateFestivalHandler _updateFestivalHandler;
    private readonly DeleteFestivalHandler _deleteFestivalHandler;
    private readonly IValidator<CreateFestivalCommand> _createFestivalValidator;
    private readonly IValidator<UpdateFestivalCommand> _updateFestivalValidator;

    public FestivalService(
        CreateFestivalHandler createFestivalHandler,
        GetFestivalByIdHandler getFestivalByIdHandler,
        GetFestivalsHandler getFestivalsHandler,
        UpdateFestivalHandler updateFestivalHandler,
        DeleteFestivalHandler deleteFestivalHandler,
    IValidator<CreateFestivalCommand> createFestivalValidator,
        IValidator<UpdateFestivalCommand> updateFestivalValidator)
    {
        _createFestivalHandler = createFestivalHandler;
        _createFestivalValidator = createFestivalValidator;
        _getFestivalByIdHandler = getFestivalByIdHandler;
        _getFestivalsHandler = getFestivalsHandler;
        _updateFestivalHandler = updateFestivalHandler;
        _updateFestivalValidator = updateFestivalValidator;
        _deleteFestivalHandler = deleteFestivalHandler;
    }

    public async Task<ApiResponse<CreateFestivalResponse>> CreateAsync(CreateFestivalCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createFestivalValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        FestivalDto festivalDto = await _createFestivalHandler.HandleAsync(command, cancellationToken);

        ApiResponse<CreateFestivalResponse> response = new()
        {
            Success = true,
            Data = new CreateFestivalResponse()
            {
                Festival = festivalDto,
            },
            Errors = new List<string>(),
            Message = $"{festivalDto.Name} is correctly registered",
        };

        return response;
    }

    public async Task<ApiResponse<GetFestivalByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetFestivalByIdQuery query = new(id);

        FestivalDto? festivalDto = await _getFestivalByIdHandler.HandleAsync(query, cancellationToken);

        if (festivalDto is null)
        {
            throw new NotFoundException($"Festival with id '{id}' was not found.");
        }

        ApiResponse<GetFestivalByIdResponse> response = new()
        {
            Success = true,
            Data = new GetFestivalByIdResponse()
            {
                Festival = festivalDto,
            },
            Errors = new List<string>(),
            Message = $"{festivalDto.Name} it's in our system",
        };

        return response;
    }

    public async Task<ApiResponse<GetFestivalsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        GetFestivalsQuery query = new();

        IReadOnlyList<FestivalDto> festivalDtos = await _getFestivalsHandler.HandleAsync(query, cancellationToken);

        if (festivalDtos is null)
        {
            throw new NotFoundException($"There are no festivals created yet.");
        }

        ApiResponse<GetFestivalsResponse> response = new()
        {
            Success = true,
            Data = new GetFestivalsResponse()
            {
                Festivals = festivalDtos,
            },
            Errors = new List<string>(),
            Message = $"There are {festivalDtos.Count} festivals registered",
        };

        return response;
    }

    public async Task<ApiResponse<UpdateFestivalResponse>> UpdateAsync(UpdateFestivalCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult = await _updateFestivalValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        FestivalDto festivalDto = await _updateFestivalHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateFestivalResponse>
        {
            Success = true,
            Message = "Festival updated successfully.",
            Data = new UpdateFestivalResponse
            {
                Festival = festivalDto
            },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteFestivalResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteFestivalCommand command = new(id);

        Guid deletedId = await _deleteFestivalHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteFestivalResponse>
        {
            Success = true,
            Message = "Festival deleted successfully.",
            Data = new DeleteFestivalResponse
            {
                Id = deletedId,
                Deleted = true
            },
            Errors = []
        };
    }
}
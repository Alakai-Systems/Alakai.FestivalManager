using Alakai.FestivalManager.Application.Features.Festivals.Contracts.DTOs;
using Alakai.FestivalManager.Application.Interfaces.Repositories;

namespace Alakai.FestivalManager.Application.Features.Festivals.Services;

public class FestivalService : IFestivalService
{
    private readonly CreateFestivalHandler _createFestivalHandler;
    private readonly GetFestivalByIdHandler _getFestivalByIdHandler;
    private readonly GetFestivalsHandler _getFestivalsHandler;
    private readonly IValidator<CreateFestivalCommand> _createFestivalValidator;

    public FestivalService(
        CreateFestivalHandler createFestivalHandler,
        GetFestivalByIdHandler getFestivalByIdHandler,
        GetFestivalsHandler getFestivalsHandler,
        IValidator<CreateFestivalCommand> createFestivalValidator)
    {
        _createFestivalHandler = createFestivalHandler;
        _createFestivalValidator = createFestivalValidator;
        _getFestivalByIdHandler = getFestivalByIdHandler;
        _getFestivalsHandler = getFestivalsHandler;
    }

    public async Task<ApiResponse<CreateFestivalResponse>> CreateAsync(CreateFestivalCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createFestivalValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        FestivalDto festivalDto = await _createFestivalHandler.HandleAsync(command, cancellationToken);

        if (String.Equals(festivalDto.Slug, command.Slug))
        {
            throw new BusinessRuleException(
                $"A festival with slug '{command.Slug}' already exists.");
        }

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
        GetFestivalByIdQuery query = new (id);

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
        GetFestivalsQuery query = new ();

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
}
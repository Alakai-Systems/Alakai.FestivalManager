
namespace Alakai.FestivalManager.Application.Features.Festivals.Services;

public class FestivalService : IFestivalService
{
    private readonly CreateFestivalHandler _createFestivalHandler;
    private readonly GetFestivalByIdHandler _getFestivalByIdHandler;
    private readonly IValidator<CreateFestivalCommand> _createFestivalValidator;

    public FestivalService(
        CreateFestivalHandler createFestivalHandler,
        GetFestivalByIdHandler getFestivalByIdHandler,
        IValidator<CreateFestivalCommand> createFestivalValidator)
    {
        _createFestivalHandler = createFestivalHandler;
        _createFestivalValidator = createFestivalValidator;
        _getFestivalByIdHandler = getFestivalByIdHandler;
    }

    public async Task<FestivalDto> CreateAsync(CreateFestivalCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createFestivalValidator.ValidateAsync(
            command,
            cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        return await _createFestivalHandler.HandleAsync(
            command,
            cancellationToken);
    }

    public async Task<FestivalDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetFestivalByIdQuery query = new (id);

        FestivalDto? festivalDto = await _getFestivalByIdHandler.HandleAsync(query, cancellationToken);

        return festivalDto;
    }
}
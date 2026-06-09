namespace Alakai.FestivalManager.Application.Features.Festivals.Services;

public class FestivalService : IFestivalService
{
    private readonly CreateFestivalHandler _createFestivalHandler;
    private readonly IValidator<CreateFestivalCommand> _createFestivalValidator;

    public FestivalService(
        CreateFestivalHandler createFestivalHandler,
        IValidator<CreateFestivalCommand> createFestivalValidator)
    {
        _createFestivalHandler = createFestivalHandler;
        _createFestivalValidator = createFestivalValidator;
    }

    public async Task<FestivalDto> CreateAsync(
        CreateFestivalCommand command,
        CancellationToken cancellationToken = default)
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
}
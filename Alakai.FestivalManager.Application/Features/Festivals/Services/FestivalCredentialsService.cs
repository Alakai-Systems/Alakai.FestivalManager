namespace Alakai.FestivalManager.Application.Features.Festivals.Services;

public class FestivalCredentialsService : IFestivalCredentialsService
{
    private readonly GetFestivalCredentialsByFestivalIdHandler _getHandler;
    private readonly UpsertFestivalCredentialsHandler _upsertHandler;
    private readonly IValidator<UpsertFestivalCredentialsCommand> _upsertValidator;

    public FestivalCredentialsService(
        GetFestivalCredentialsByFestivalIdHandler getHandler,
        UpsertFestivalCredentialsHandler upsertHandler,
        IValidator<UpsertFestivalCredentialsCommand> upsertValidator)
    {
        _getHandler = getHandler;
        _upsertHandler = upsertHandler;
        _upsertValidator = upsertValidator;
    }

    public async Task<ApiResponse<GetFestivalCredentialsResponse>> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default)
    {
        FestivalCredentialsDto? credentialsDto = await _getHandler.HandleAsync(festivalId, cancellationToken);

        return new ApiResponse<GetFestivalCredentialsResponse>
        {
            Success = true,
            Message = credentialsDto is null ? "No payment settings configured yet for this festival." : "Payment settings loaded.",
            Data = new GetFestivalCredentialsResponse { Credentials = credentialsDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpsertFestivalCredentialsResponse>> UpsertAsync(UpsertFestivalCredentialsCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _upsertValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        FestivalCredentialsDto credentialsDto = await _upsertHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpsertFestivalCredentialsResponse>
        {
            Success = true,
            Message = "Payment settings saved successfully.",
            Data = new UpsertFestivalCredentialsResponse { Credentials = credentialsDto },
            Errors = []
        };
    }
}
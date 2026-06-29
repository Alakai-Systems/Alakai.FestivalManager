namespace Alakai.FestivalManager.Application.Features.CompetitionEntries.Services;

public class CompetitionEntryService : ICompetitionEntryService
{
    private readonly CreateCompetitionEntryHandler _createCompetitionEntryHandler;
    private readonly GetCompetitionEntryByIdHandler _getCompetitionEntryByIdHandler;
    private readonly GetCompetitionEntriesHandler _getCompetitionEntriesHandler;
    private readonly GetCompetitionEntriesByCompetitionIdHandler _getCompetitionEntriesByCompetitionIdHandler;
    private readonly GetCompetitionEntriesByRegistrationIdHandler _getCompetitionEntriesByRegistrationIdHandler;
    private readonly UpdateCompetitionEntryHandler _updateCompetitionEntryHandler;
    private readonly DeleteCompetitionEntryHandler _deleteCompetitionEntryHandler;
    private readonly IValidator<CreateCompetitionEntryCommand> _createCompetitionEntryValidator;
    private readonly IValidator<UpdateCompetitionEntryCommand> _updateCompetitionEntryValidator;
    private readonly IEmailNotificationService _emailNotificationService;

    public CompetitionEntryService(CreateCompetitionEntryHandler createCompetitionEntryHandler, GetCompetitionEntryByIdHandler getCompetitionEntryByIdHandler, 
        GetCompetitionEntriesHandler getCompetitionEntriesHandler, GetCompetitionEntriesByCompetitionIdHandler getCompetitionEntriesByCompetitionIdHandler, 
        GetCompetitionEntriesByRegistrationIdHandler getCompetitionEntriesByRegistrationIdHandler, UpdateCompetitionEntryHandler updateCompetitionEntryHandler, 
        DeleteCompetitionEntryHandler deleteCompetitionEntryHandler, IValidator<CreateCompetitionEntryCommand> createCompetitionEntryValidator, 
        IValidator<UpdateCompetitionEntryCommand> updateCompetitionEntryValidator, IEmailNotificationService emailNotificationService)
    {
        _createCompetitionEntryHandler = createCompetitionEntryHandler;
        _getCompetitionEntryByIdHandler = getCompetitionEntryByIdHandler;
        _getCompetitionEntriesHandler = getCompetitionEntriesHandler;
        _getCompetitionEntriesByCompetitionIdHandler = getCompetitionEntriesByCompetitionIdHandler;
        _getCompetitionEntriesByRegistrationIdHandler = getCompetitionEntriesByRegistrationIdHandler;
        _updateCompetitionEntryHandler = updateCompetitionEntryHandler;
        _deleteCompetitionEntryHandler = deleteCompetitionEntryHandler;
        _createCompetitionEntryValidator = createCompetitionEntryValidator;
        _updateCompetitionEntryValidator = updateCompetitionEntryValidator;
        _emailNotificationService = emailNotificationService;
    }

    public async Task<ApiResponse<CreateCompetitionEntryResponse>> CreateAsync(CreateCompetitionEntryCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createCompetitionEntryValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        CompetitionEntryDto competitionEntryDto = await _createCompetitionEntryHandler.HandleAsync(command, cancellationToken);

        await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.CompetitionEntryConfirmed, competitionEntryDto.RegistrationId, cancellationToken);

        ApiResponse<CreateCompetitionEntryResponse> response = new()
        {
            Success = true,
            Data = new CreateCompetitionEntryResponse
            {
                CompetitionEntry = competitionEntryDto
            },
            Errors = [],
            Message = "Competition entry registered successfully"
        };

        return response;
    }

    public async Task<ApiResponse<GetCompetitionEntryByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        CompetitionEntryDto competitionEntryDto = await _getCompetitionEntryByIdHandler.HandleAsync(new GetCompetitionEntryByIdQuery { Id = id }, cancellationToken);

        ApiResponse<GetCompetitionEntryByIdResponse> response = new()
        {
            Success = true,
            Data = new GetCompetitionEntryByIdResponse
            {
                CompetitionEntry = competitionEntryDto
            },
            Errors = [],
            Message = "Competition entry retrieved successfully"
        };

        return response;
    }

    public async Task<ApiResponse<GetCompetitionEntriesResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntryDto> competitionEntries = await _getCompetitionEntriesHandler.HandleAsync(cancellationToken);

        ApiResponse<GetCompetitionEntriesResponse> response = new()
        {
            Success = true,
            Data = new GetCompetitionEntriesResponse
            {
                CompetitionEntries = competitionEntries
            },
            Errors = [],
            Message = $"There are {competitionEntries.Count} competition entries registered"
        };

        return response;
    }

    public async Task<ApiResponse<GetCompetitionEntriesByCompetitionIdResponse>> GetByCompetitionIdAsync(Guid competitionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntryDto> competitionEntries = await _getCompetitionEntriesByCompetitionIdHandler.HandleAsync(new GetCompetitionEntriesByCompetitionIdQuery { CompetitionId = competitionId }, cancellationToken);

        ApiResponse<GetCompetitionEntriesByCompetitionIdResponse> response = new()
        {
            Success = true,
            Data = new GetCompetitionEntriesByCompetitionIdResponse
            {
                CompetitionEntries = competitionEntries
            },
            Errors = [],
            Message = $"There are {competitionEntries.Count} entries for this competition"
        };

        return response;
    }

    public async Task<ApiResponse<GetCompetitionEntriesByRegistrationIdResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionEntryDto> competitionEntries = await _getCompetitionEntriesByRegistrationIdHandler.HandleAsync(new GetCompetitionEntriesByRegistrationIdQuery { RegistrationId = registrationId }, cancellationToken);

        ApiResponse<GetCompetitionEntriesByRegistrationIdResponse> response = new()
        {
            Success = true,
            Data = new GetCompetitionEntriesByRegistrationIdResponse
            {
                CompetitionEntries = competitionEntries
            },
            Errors = [],
            Message = $"There are {competitionEntries.Count} entries for this registration"
        };

        return response;
    }

    public async Task<ApiResponse<UpdateCompetitionEntryResponse>> UpdateAsync(Guid id, UpdateCompetitionEntryCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;

        ValidationResult validationResult = await _updateCompetitionEntryValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        CompetitionEntryDto competitionEntryDto = await _updateCompetitionEntryHandler.HandleAsync(command, cancellationToken);

        await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.CompetitionEntryConfirmed, competitionEntryDto.RegistrationId, cancellationToken);

        ApiResponse<UpdateCompetitionEntryResponse> response = new()
        {
            Success = true,
            Data = new UpdateCompetitionEntryResponse
            {
                CompetitionEntry = competitionEntryDto
            },
            Errors = [],
            Message = "Competition entry updated successfully"
        };

        return response;
    }

    public async Task<ApiResponse<DeleteCompetitionEntryResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        CompetitionEntryDto existingEntryDto = await _getCompetitionEntryByIdHandler.HandleAsync(new GetCompetitionEntryByIdQuery { Id = id }, cancellationToken);

        await _deleteCompetitionEntryHandler.HandleAsync(new DeleteCompetitionEntryCommand { Id = id }, cancellationToken);

        await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.CompetitionEntryCancelled, existingEntryDto.RegistrationId, cancellationToken);

        ApiResponse<DeleteCompetitionEntryResponse> response = new()
        {
            Success = true,
            Data = new DeleteCompetitionEntryResponse
            {
                Id = id,
                Deleted = true
            },
            Errors = [],
            Message = "Competition entry deleted successfully"
        };

        return response;
    }
}
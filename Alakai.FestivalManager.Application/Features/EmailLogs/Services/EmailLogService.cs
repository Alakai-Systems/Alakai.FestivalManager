namespace Alakai.FestivalManager.Application.Features.EmailLogs.Services;

public class EmailLogService : IEmailLogService
{
    private readonly CreateEmailLogHandler _createEmailLogHandler;
    private readonly GetEmailLogByIdHandler _getEmailLogByIdHandler;
    private readonly GetEmailLogsHandler _getEmailLogsHandler;
    private readonly GetEmailLogsByEditionIdHandler _getEmailLogsByEditionIdHandler;
    private readonly GetEmailLogsByRegistrationIdHandler _getEmailLogsByRegistrationIdHandler;
    private readonly GetEmailLogsByUserIdHandler _getEmailLogsByUserIdHandler;
    private readonly UpdateEmailLogHandler _updateEmailLogHandler;
    private readonly DeleteEmailLogHandler _deleteEmailLogHandler;
    private readonly IValidator<CreateEmailLogCommand> _createEmailLogValidator;
    private readonly IValidator<UpdateEmailLogCommand> _updateEmailLogValidator;

    public EmailLogService(CreateEmailLogHandler createEmailLogHandler, GetEmailLogByIdHandler getEmailLogByIdHandler, GetEmailLogsHandler getEmailLogsHandler, GetEmailLogsByEditionIdHandler getEmailLogsByEditionIdHandler, GetEmailLogsByRegistrationIdHandler getEmailLogsByRegistrationIdHandler, GetEmailLogsByUserIdHandler getEmailLogsByUserIdHandler, UpdateEmailLogHandler updateEmailLogHandler, DeleteEmailLogHandler deleteEmailLogHandler, IValidator<CreateEmailLogCommand> createEmailLogValidator, IValidator<UpdateEmailLogCommand> updateEmailLogValidator)
    {
        _createEmailLogHandler = createEmailLogHandler;
        _getEmailLogByIdHandler = getEmailLogByIdHandler;
        _getEmailLogsHandler = getEmailLogsHandler;
        _getEmailLogsByEditionIdHandler = getEmailLogsByEditionIdHandler;
        _getEmailLogsByRegistrationIdHandler = getEmailLogsByRegistrationIdHandler;
        _getEmailLogsByUserIdHandler = getEmailLogsByUserIdHandler;
        _updateEmailLogHandler = updateEmailLogHandler;
        _deleteEmailLogHandler = deleteEmailLogHandler;
        _createEmailLogValidator = createEmailLogValidator;
        _updateEmailLogValidator = updateEmailLogValidator;
    }

    public async Task<ApiResponse<CreateEmailLogResponse>> CreateAsync(CreateEmailLogCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createEmailLogValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        EmailLogDto emailLogDto = await _createEmailLogHandler.HandleAsync(command, cancellationToken);

        ApiResponse<CreateEmailLogResponse> response = new() 
        { 
            Success = true, Data = new CreateEmailLogResponse { EmailLog = emailLogDto }, 
            Errors = [], 
            Message = "Email log created successfully" 
        };

        return response;
    }

    public async Task<ApiResponse<GetEmailLogByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EmailLogDto emailLogDto = await _getEmailLogByIdHandler.HandleAsync(new GetEmailLogByIdQuery { Id = id }, cancellationToken);

        ApiResponse<GetEmailLogByIdResponse> response = new() 
        { 
            Success = true, Data = new GetEmailLogByIdResponse { EmailLog = emailLogDto }, 
            Errors = [], 
            Message = "Email log retrieved successfully" 
        };

        return response;
    }

    public async Task<ApiResponse<GetEmailLogsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLogDto> emailLogs = await _getEmailLogsHandler.HandleAsync(cancellationToken);

        ApiResponse<GetEmailLogsResponse> response = new() 
        { 
            Success = true,
            Data = new GetEmailLogsResponse { EmailLogs = emailLogs }, 
            Errors = [], 
            Message = $"There are {emailLogs.Count} email logs registered" 
        };

        return response;
    }

    public async Task<ApiResponse<GetEmailLogsByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLogDto> emailLogs = await _getEmailLogsByEditionIdHandler.HandleAsync(new GetEmailLogsByEditionIdQuery { EditionId = editionId }, cancellationToken);

        ApiResponse<GetEmailLogsByEditionIdResponse> response = new() 
        { 
            Success = true, 
            Data = new GetEmailLogsByEditionIdResponse { EmailLogs = emailLogs }, 
            Errors = [], 
            Message = $"There are {emailLogs.Count} email logs for this edition" 
        };

        return response;
    }

    public async Task<ApiResponse<GetEmailLogsByRegistrationIdResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLogDto> emailLogs = await _getEmailLogsByRegistrationIdHandler.HandleAsync(new GetEmailLogsByRegistrationIdQuery { RegistrationId = registrationId }, cancellationToken);
        
        ApiResponse<GetEmailLogsByRegistrationIdResponse> response = new() 
        { 
            Success = true, 
            Data = new GetEmailLogsByRegistrationIdResponse { EmailLogs = emailLogs }, 
            Errors = [], 
            Message = $"There are {emailLogs.Count} email logs for this registration" 
        };

        return response;
    }

    public async Task<ApiResponse<GetEmailLogsByUserIdResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLogDto> emailLogs = await _getEmailLogsByUserIdHandler.HandleAsync(new GetEmailLogsByUserIdQuery { UserId = userId }, cancellationToken);
        
        ApiResponse<GetEmailLogsByUserIdResponse> response = new() 
        { 
            Success = true, 
            Data = new GetEmailLogsByUserIdResponse { EmailLogs = emailLogs }, 
            Errors = [], 
            Message = $"There are {emailLogs.Count} email logs for this user" 
        };

        return response;
    }

    public async Task<ApiResponse<UpdateEmailLogResponse>> UpdateAsync(Guid id, UpdateEmailLogCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;
        ValidationResult validationResult = await _updateEmailLogValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        EmailLogDto emailLogDto = await _updateEmailLogHandler.HandleAsync(command, cancellationToken);
        
        ApiResponse<UpdateEmailLogResponse> response = new() 
        { 
            Success = true, 
            Data = new UpdateEmailLogResponse { EmailLog = emailLogDto }, 
            Errors = [], 
            Message = "Email log updated successfully" 
        };

        return response;
    }

    public async Task<ApiResponse<DeleteEmailLogResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _deleteEmailLogHandler.HandleAsync(new DeleteEmailLogCommand { Id = id }, cancellationToken);

        ApiResponse<DeleteEmailLogResponse> response = new() 
        { 
            Success = true, 
            Data = new DeleteEmailLogResponse { Id = id, Deleted = true }, 
            Errors = [], 
            Message = "Email log deleted successfully" 
        };

        return response;
    }
}

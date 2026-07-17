namespace Alakai.FestivalManager.Application.Features.EmailLayouts.Services;

public class EmailLayoutService : IEmailLayoutService
{
    private readonly IEmailLayoutRepository _emailLayoutRepository;

    public EmailLayoutService(IEmailLayoutRepository emailLayoutRepository)
    {
        _emailLayoutRepository = emailLayoutRepository;
    }

    public async Task<ApiResponse<GetEmailLayoutsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmailLayout> layouts = await _emailLayoutRepository.GetAllAsync(cancellationToken);

        List<EmailLayoutDto> dtos = layouts.Select(ToDto).ToList();

        return new ApiResponse<GetEmailLayoutsResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} email layouts.",
            Data = new GetEmailLayoutsResponse { EmailLayouts = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<CreateEmailLayoutResponse>> CreateAsync(CreateEmailLayoutRequest request, CancellationToken cancellationToken = default)
    {
        EmailLayout emailLayout = new()
        {
            EditionId = request.EditionId,
            Name = request.Name,
            HeaderHtml = request.HeaderHtml,
            HeaderText = request.HeaderText,
            FooterHtml = request.FooterHtml,
            FooterText = request.FooterText,
            HeaderImageWidth = request.HeaderImageWidth,
            FooterImageWidth = request.FooterImageWidth,
            IsActive = request.IsActive
        };

        await _emailLayoutRepository.AddAsync(emailLayout, cancellationToken);
        await _emailLayoutRepository.SaveChangesAsync(cancellationToken);

        EmailLayout? saved = await _emailLayoutRepository.GetByIdAsync(emailLayout.Id, cancellationToken);

        return new ApiResponse<CreateEmailLayoutResponse>
        {
            Success = true,
            Message = "Email layout created successfully.",
            Data = new CreateEmailLayoutResponse { EmailLayout = ToDto(saved ?? emailLayout) },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateEmailLayoutResponse>> UpdateAsync(Guid id, UpdateEmailLayoutRequest request, CancellationToken cancellationToken = default)
    {
        EmailLayout? emailLayout = await _emailLayoutRepository.GetByIdAsync(id, cancellationToken);

        if (emailLayout is null)
        {
            return new ApiResponse<UpdateEmailLayoutResponse>
            {
                Success = false,
                Message = "Email layout not found.",
                Data = null,
                Errors = ["Email layout not found."]
            };
        }

        emailLayout.EditionId = request.EditionId;
        emailLayout.Name = request.Name;
        emailLayout.HeaderHtml = request.HeaderHtml;
        emailLayout.HeaderText = request.HeaderText;
        emailLayout.FooterHtml = request.FooterHtml;
        emailLayout.FooterText = request.FooterText;
        emailLayout.HeaderImageWidth = request.HeaderImageWidth;
        emailLayout.FooterImageWidth = request.FooterImageWidth;
        emailLayout.IsActive = request.IsActive;
        emailLayout.SetUpdated();

        _emailLayoutRepository.Update(emailLayout);
        await _emailLayoutRepository.SaveChangesAsync(cancellationToken);

        EmailLayout? refreshed = await _emailLayoutRepository.GetByIdAsync(id, cancellationToken);

        return new ApiResponse<UpdateEmailLayoutResponse>
        {
            Success = true,
            Message = "Email layout updated successfully.",
            Data = new UpdateEmailLayoutResponse { EmailLayout = ToDto(refreshed ?? emailLayout) },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteEmailLayoutResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EmailLayout? emailLayout = await _emailLayoutRepository.GetByIdAsync(id, cancellationToken);

        if (emailLayout is null)
        {
            return new ApiResponse<DeleteEmailLayoutResponse>
            {
                Success = true,
                Message = "Email layout not found.",
                Data = new DeleteEmailLayoutResponse { Id = id, Deleted = false },
                Errors = []
            };
        }

        _emailLayoutRepository.Delete(emailLayout);
        await _emailLayoutRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<DeleteEmailLayoutResponse>
        {
            Success = true,
            Message = "Email layout deleted successfully.",
            Data = new DeleteEmailLayoutResponse { Id = id, Deleted = true },
            Errors = []
        };
    }

    private static EmailLayoutDto ToDto(EmailLayout emailLayout)
    {
        return new EmailLayoutDto
        {
            Id = emailLayout.Id,
            EditionId = emailLayout.EditionId,
            EditionName = emailLayout.Edition?.Name,
            Name = emailLayout.Name,
            HeaderHtml = emailLayout.HeaderHtml,
            HeaderText = emailLayout.HeaderText,
            FooterHtml = emailLayout.FooterHtml,
            FooterText = emailLayout.FooterText,
            HeaderImageWidth = emailLayout.HeaderImageWidth,
            FooterImageWidth = emailLayout.FooterImageWidth,
            IsActive = emailLayout.IsActive,
            CreatedAt = emailLayout.CreatedAt,
            UpdatedAt = emailLayout.UpdatedAt
        };
    }
}
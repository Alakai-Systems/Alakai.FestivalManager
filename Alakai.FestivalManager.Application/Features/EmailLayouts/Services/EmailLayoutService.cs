namespace Alakai.FestivalManager.Application.Features.EmailLayouts.Services;

public class EmailLayoutService : IEmailLayoutService
{
    private readonly IEmailLayoutRepository _emailLayoutRepository;

    public EmailLayoutService(IEmailLayoutRepository emailLayoutRepository)
    {
        _emailLayoutRepository = emailLayoutRepository;
    }

    public async Task<ApiResponse<GetEmailLayoutResponse>> GetAsync(CancellationToken cancellationToken = default)
    {
        EmailLayout? emailLayout = await _emailLayoutRepository.GetAsync(cancellationToken);

        EmailLayoutDto dto = emailLayout is null ? new EmailLayoutDto() : ToDto(emailLayout);

        return new ApiResponse<GetEmailLayoutResponse>
        {
            Success = true,
            Message = "Email layout retrieved successfully",
            Data = new GetEmailLayoutResponse { EmailLayout = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateEmailLayoutResponse>> UpdateAsync(UpdateEmailLayoutRequest request, CancellationToken cancellationToken = default)
    {
        EmailLayout? emailLayout = await _emailLayoutRepository.GetAsync(cancellationToken);

        if (emailLayout is null)
        {
            emailLayout = new EmailLayout
            {
                HeaderHtml = request.HeaderHtml,
                HeaderText = request.HeaderText,
                FooterHtml = request.FooterHtml,
                FooterText = request.FooterText
            };

            await _emailLayoutRepository.AddAsync(emailLayout, cancellationToken);
        }
        else
        {
            emailLayout.HeaderHtml = request.HeaderHtml;
            emailLayout.HeaderText = request.HeaderText;
            emailLayout.FooterHtml = request.FooterHtml;
            emailLayout.FooterText = request.FooterText;
            emailLayout.SetUpdated();

            _emailLayoutRepository.Update(emailLayout);
        }

        await _emailLayoutRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<UpdateEmailLayoutResponse>
        {
            Success = true,
            Message = "Email layout updated successfully",
            Data = new UpdateEmailLayoutResponse { EmailLayout = ToDto(emailLayout) },
            Errors = []
        };
    }

    private static EmailLayoutDto ToDto(EmailLayout emailLayout)
    {
        return new EmailLayoutDto
        {
            Id = emailLayout.Id,
            HeaderHtml = emailLayout.HeaderHtml,
            HeaderText = emailLayout.HeaderText,
            FooterHtml = emailLayout.FooterHtml,
            FooterText = emailLayout.FooterText,
            CreatedAt = emailLayout.CreatedAt,
            UpdatedAt = emailLayout.UpdatedAt
        };
    }
}
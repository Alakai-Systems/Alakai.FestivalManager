namespace Alakai.FestivalManager.Application.Features.EmailLogs.Validators;

public class UpdateEmailLogCommandValidator : AbstractValidator<UpdateEmailLogCommand>
{
    public UpdateEmailLogCommandValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
        RuleFor(e => e.TemplateKey).IsInEnum();
        RuleFor(e => e.RecipientEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(e => e.RecipientName).MaximumLength(200);
        RuleFor(e => e.Subject).NotEmpty().MaximumLength(300);
        RuleFor(e => e.BodyHtml).NotEmpty();
        RuleFor(e => e.Status).IsInEnum();
        RuleFor(e => e.ErrorMessage).MaximumLength(2000);
    }
}

using Alakai.FestivalManager.Application.Features.Users.Commands.CreateAdminUser;
using Alakai.FestivalManager.Domain.Enums;
using FluentValidation;

namespace Alakai.FestivalManager.Application.Features.Users.Validators;

public class CreateAdminUserCommandValidator : AbstractValidator<CreateAdminUserCommand>
{
    public CreateAdminUserCommandValidator()
    {
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(command => command.Role)
            .Must(role => role == UserRole.Admin || role == UserRole.SuperAdmin)
            .WithMessage("Role must be Admin or SuperAdmin.");
    }
}
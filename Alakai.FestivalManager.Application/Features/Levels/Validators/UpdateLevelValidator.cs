namespace Alakai.FestivalManager.Application.Features.Levels.Validators;

public class UpdateLevelValidator : AbstractValidator<UpdateLevelCommand>
{
    public UpdateLevelValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PassTypeId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.EarlyBirdPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GroupPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RegularPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LeaderCapacity).GreaterThanOrEqualTo(0).When(x => x.LeaderCapacity.HasValue);
        RuleFor(x => x.FollowerCapacity).GreaterThanOrEqualTo(0).When(x => x.FollowerCapacity.HasValue);
        RuleFor(x => x.IndividualCapacity).GreaterThanOrEqualTo(0).When(x => x.IndividualCapacity.HasValue);
        RuleFor(x => x.MaxLeaderDifference).GreaterThanOrEqualTo(0).When(x => x.MaxLeaderDifference.HasValue);
        RuleFor(x => x.MaxFollowerDifference).GreaterThanOrEqualTo(0).When(x => x.MaxFollowerDifference.HasValue);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
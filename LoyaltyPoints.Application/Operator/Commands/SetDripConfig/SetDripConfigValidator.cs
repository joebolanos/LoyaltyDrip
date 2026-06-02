using FluentValidation;

namespace LoyaltyPoints.Application.Operator.Commands.SetDripConfig;

public class SetDripConfigValidator : AbstractValidator<SetDripConfigCommand>
{
    public SetDripConfigValidator()
    {
        RuleFor(x => x.ImmediateCreditRatio)
            .GreaterThan(0)
            .WithMessage("ImmediateCreditRatio must be greater than 0");

        RuleFor(x => x.DripPoolRatio)
            .GreaterThan(0)
            .WithMessage("DripPoolRatio must be greater than 0");

        RuleFor(x => x.DailyAllotmentPercent)
            .GreaterThan(0)
            .WithMessage("DailyAllotmentPercent must be greater than 0")
            .LessThanOrEqualTo(1)
            .WithMessage("DailyAllotmentPercent must be <= 1");

        RuleFor(x => x.MinimumBalanceForClaim)
            .GreaterThan(0)
            .WithMessage("MinimumBalanceForClaim must be greater than 0");

        RuleFor(x => x.ChangedBy)
            .NotEmpty()
            .WithMessage("ChangedBy is required");
    }
}

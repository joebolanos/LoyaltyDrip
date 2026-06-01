using FluentValidation;

namespace LoyaltyPoints.Application.Drip.Commands.ClaimDailyAllotment;

public class ClaimDailyAllotmentValidator : AbstractValidator<ClaimDailyAllotmentCommand>
{
    public ClaimDailyAllotmentValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required");
    }
}

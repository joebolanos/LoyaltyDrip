using FluentValidation;

namespace LoyaltyPoints.Application.Jobs.NightlyDripJob;

public class RunNightlyDripJobValidator : AbstractValidator<RunNightlyDripJobCommand>
{
    public RunNightlyDripJobValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required");
    }
}

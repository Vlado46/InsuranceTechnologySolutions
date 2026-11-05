using Claims.Application.Contracts;
using Claims.Application.Interfaces;
using Claims.Application.Services;
using FluentValidation;

namespace Claims.Application.Validation;

public class CreateClaimRequestValidator : AbstractValidator<CreateClaimRequest>
{
    public const decimal MaxDamageCost = 100_000m;

    public CreateClaimRequestValidator(ICoverRepository covers)
    {
        RuleFor(claim => claim.Name).NotEmpty();
        RuleFor(claim => claim.Type).IsInEnum();
        RuleFor(claim => claim.DamageCost).GreaterThan(0).LessThanOrEqualTo(MaxDamageCost);

        RuleFor(claim => claim.CoverId)
            .NotEmpty()
            .DependentRules(() => RuleFor(claim => claim).CustomAsync(async (claim, context, cancellationToken) =>
            {
                var cover = await covers.GetByIdAsync(claim.CoverId, cancellationToken);
                if (cover is null)
                {
                    context.AddFailure(nameof(claim.CoverId), "Cover does not exist.");
                    return;
                }

                var created = claim.Created.AsUtc().Date;
                if (created < cover.StartDate.Date || created > cover.EndDate.Date)
                {
                    context.AddFailure(nameof(claim.Created), "Created date must be within the period of the cover.");
                }
            }));
    }
}

using Claims.Application.Contracts;
using Claims.Application.Services;
using FluentValidation;

namespace Claims.Application.Validation;

public class CreateCoverRequestValidator : AbstractValidator<CreateCoverRequest>
{
    public CreateCoverRequestValidator(TimeProvider timeProvider)
    {
        RuleFor(cover => cover.Type).IsInEnum();

        RuleFor(cover => cover.StartDate)
            .Must(startDate => startDate.AsUtc().Date >= timeProvider.GetUtcNow().UtcDateTime.Date)
            .WithMessage("Start date cannot be in the past.");

        RuleFor(cover => cover.EndDate)
            .Must((cover, endDate) => endDate.AsUtc().Date > cover.StartDate.AsUtc().Date)
            .WithMessage("End date must be after the start date.")
            .Must((cover, endDate) => IsWithinOneYear(cover.StartDate.AsUtc().Date, endDate.AsUtc().Date))
            .WithMessage("The insurance period cannot exceed 1 year.");
    }

    // A calendar year, so a period over 29 February can take 366 days.
    private static bool IsWithinOneYear(DateTime startDate, DateTime endDate) =>
        startDate.Year < DateTime.MaxValue.Year && endDate <= startDate.AddYears(1);
}

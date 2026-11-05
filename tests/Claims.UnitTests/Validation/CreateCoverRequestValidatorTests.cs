using Claims.Application.Contracts;
using Claims.Application.Validation;
using Claims.Domain;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Claims.UnitTests.Validation;

public class CreateCoverRequestValidatorTests
{
    private static readonly DateTime Today = new(2026, 6, 15);

    private readonly CreateCoverRequestValidator _validator = new(
        new FakeTimeProvider(new DateTimeOffset(Today.AddHours(12), TimeSpan.Zero)));

    [Fact]
    public void Valid_WhenStartingToday()
    {
        var result = _validator.Validate(new CreateCoverRequest(Today, Today.AddDays(30), CoverType.Tanker));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Invalid_WhenStartDateIsInThePast()
    {
        var result = _validator.Validate(new CreateCoverRequest(Today.AddDays(-1), Today.AddDays(30), CoverType.Tanker));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCoverRequest.StartDate));
    }

    [Fact]
    public void Invalid_WhenEndDateIsNotAfterStartDate()
    {
        var result = _validator.Validate(new CreateCoverRequest(Today, Today.AddHours(6), CoverType.Tanker));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCoverRequest.EndDate));
    }

    [Fact]
    public void Valid_WhenPeriodIsExactlyOneYear()
    {
        var result = _validator.Validate(new CreateCoverRequest(Today, Today.AddYears(1), CoverType.Tanker));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Valid_WhenOneYearIncludesALeapDay()
    {
        var startDate = new DateTime(2027, 6, 15);
        var endDate = startDate.AddYears(1);

        var result = _validator.Validate(new CreateCoverRequest(startDate, endDate, CoverType.Tanker));

        Assert.Equal(366, (endDate - startDate).Days);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Invalid_WhenPeriodExceedsOneYear()
    {
        var result = _validator.Validate(new CreateCoverRequest(Today, Today.AddYears(1).AddDays(1), CoverType.Tanker));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCoverRequest.EndDate));
    }

    [Fact]
    public void Invalid_InsteadOfThrowing_WhenStartDateIsInTheLastSupportedYear()
    {
        var result = _validator.Validate(new CreateCoverRequest(new DateTime(9999, 12, 30), DateTime.MaxValue, CoverType.Tanker));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCoverRequest.EndDate));
    }

    [Fact]
    public void Invalid_WhenCoverTypeIsUnknown()
    {
        var result = _validator.Validate(new CreateCoverRequest(Today, Today.AddDays(30), (CoverType)99));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCoverRequest.Type));
    }
}

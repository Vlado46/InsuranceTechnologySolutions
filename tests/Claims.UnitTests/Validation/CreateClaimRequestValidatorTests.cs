using Claims.Application.Contracts;
using Claims.Application.Interfaces;
using Claims.Application.Validation;
using Claims.Domain;
using FluentValidation.Results;
using NSubstitute;
using Xunit;

namespace Claims.UnitTests.Validation;

public class CreateClaimRequestValidatorTests
{
    private const string CoverId = "cover-1";

    private static readonly DateTime CoverStart = new(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CoverEnd = new(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly ICoverRepository _covers = Substitute.For<ICoverRepository>();
    private readonly CreateClaimRequestValidator _validator;

    public CreateClaimRequestValidatorTests()
    {
        _covers.GetByIdAsync(CoverId, Arg.Any<CancellationToken>())
            .Returns(new Cover { Id = CoverId, StartDate = CoverStart, EndDate = CoverEnd });
        _validator = new CreateClaimRequestValidator(_covers);
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("100000")]
    public async Task Valid_WhenDamageCostIsWithinLimits(string damageCost)
    {
        var result = await ValidateAsync(ValidRequest() with { DamageCost = decimal.Parse(damageCost) });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("100000.01")]
    public async Task Invalid_WhenDamageCostIsOutOfLimits(string damageCost)
    {
        var result = await ValidateAsync(ValidRequest() with { DamageCost = decimal.Parse(damageCost) });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClaimRequest.DamageCost));
    }

    [Fact]
    public async Task Valid_OnFirstDayOfCover()
    {
        var result = await ValidateAsync(ValidRequest() with { Created = CoverStart });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Valid_OnLastDayOfCover_WhateverTheTime()
    {
        var result = await ValidateAsync(ValidRequest() with { Created = CoverEnd.AddHours(15) });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Invalid_WhenCreatedBeforeCover()
    {
        var result = await ValidateAsync(ValidRequest() with { Created = CoverStart.AddDays(-1) });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClaimRequest.Created));
    }

    [Fact]
    public async Task Invalid_WhenCreatedAfterCover()
    {
        var result = await ValidateAsync(ValidRequest() with { Created = CoverEnd.AddDays(1) });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClaimRequest.Created));
    }

    [Fact]
    public async Task Invalid_WhenCoverDoesNotExist()
    {
        var result = await ValidateAsync(ValidRequest() with { CoverId = "unknown" });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClaimRequest.CoverId));
    }

    [Fact]
    public async Task Invalid_WithoutLookingUpTheCover_WhenCoverIdIsEmpty()
    {
        var result = await ValidateAsync(ValidRequest() with { CoverId = "" });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClaimRequest.CoverId));
        await _covers.DidNotReceiveWithAnyArgs().GetByIdAsync(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Invalid_WhenNameIsEmpty()
    {
        var result = await ValidateAsync(ValidRequest() with { Name = "" });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClaimRequest.Name));
    }

    [Fact]
    public async Task Invalid_WhenClaimTypeIsUnknown()
    {
        var result = await ValidateAsync(ValidRequest() with { Type = (ClaimType)99 });

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateClaimRequest.Type));
    }

    private static CreateClaimRequest ValidRequest() =>
        new(CoverId, CoverStart.AddDays(10), "Hull damage", ClaimType.Collision, 50_000m);

    private Task<ValidationResult> ValidateAsync(CreateClaimRequest request) =>
        _validator.ValidateAsync(request, TestContext.Current.CancellationToken);
}

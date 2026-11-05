using Claims.Application.Contracts;
using Claims.Application.Interfaces;
using Claims.Application.Services;
using Claims.Application.Validation;
using Claims.Domain;
using FluentValidation;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Xunit;

namespace Claims.UnitTests.Services;

public class CoverServiceTests
{
    private static readonly DateTime Now = new(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime StartDate = new(2026, 8, 1);

    private readonly ICoverRepository _covers = Substitute.For<ICoverRepository>();
    private readonly IAuditQueue _auditQueue = Substitute.For<IAuditQueue>();
    private readonly CoverService _service;

    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public CoverServiceTests()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(Now));
        _service = new CoverService(_covers, new CreateCoverRequestValidator(timeProvider), _auditQueue, timeProvider);
    }

    [Fact]
    public async Task CreateAsync_StoresCoverWithComputedPremium()
    {
        var response = await _service.CreateAsync(ValidRequest(), _cancellationToken);

        Assert.Equal(41_250m, response.Premium);
        await _covers.Received(1).AddAsync(
            Arg.Is<Cover>(cover => cover.Id == response.Id && cover.Premium == 41_250m), _cancellationToken);
    }

    [Fact]
    public async Task CreateAsync_ReadsDatesWithoutOffsetAsUtc()
    {
        var response = await _service.CreateAsync(ValidRequest(), _cancellationToken);

        Assert.Equal(DateTimeKind.Utc, response.StartDate.Kind);
        Assert.Equal(DateTimeKind.Utc, response.EndDate.Kind);
    }

    [Fact]
    public async Task CreateAsync_QueuesAuditEntry()
    {
        var response = await _service.CreateAsync(ValidRequest(), _cancellationToken);

        await _auditQueue.Received(1).EnqueueAsync(
            new AuditEntry(AuditTarget.Cover, response.Id, AuditOperation.Create, Now));
    }

    [Fact]
    public async Task CreateAsync_DoesNotStoreOrAudit_WhenRequestIsInvalid()
    {
        var request = ValidRequest() with { EndDate = StartDate.AddYears(2) };

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request, _cancellationToken));

        await _covers.DidNotReceiveWithAnyArgs().AddAsync(default!, _cancellationToken);
        await _auditQueue.DidNotReceiveWithAnyArgs().EnqueueAsync(default!);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCoverDoesNotExist()
    {
        var response = await _service.GetByIdAsync("unknown", _cancellationToken);

        Assert.Null(response);
    }

    [Fact]
    public async Task ComputePremiumAsync_ReturnsPremiumWithoutStoringAnything()
    {
        var premium = await _service.ComputePremiumAsync(ValidRequest(), _cancellationToken);

        Assert.Equal(41_250m, premium);
        await _covers.DidNotReceiveWithAnyArgs().AddAsync(default!, _cancellationToken);
        await _auditQueue.DidNotReceiveWithAnyArgs().EnqueueAsync(default!);
    }

    [Fact]
    public async Task ComputePremiumAsync_Throws_WhenRequestIsInvalid()
    {
        var request = ValidRequest() with { StartDate = Now.AddDays(-1) };

        await Assert.ThrowsAsync<ValidationException>(() => _service.ComputePremiumAsync(request, _cancellationToken));
    }

    [Fact]
    public async Task DeleteAsync_QueuesAuditEntry_WhenCoverWasDeleted()
    {
        _covers.DeleteAsync("cover-1", _cancellationToken).Returns(true);

        var deleted = await _service.DeleteAsync("cover-1", _cancellationToken);

        Assert.True(deleted);
        await _auditQueue.Received(1).EnqueueAsync(
            new AuditEntry(AuditTarget.Cover, "cover-1", AuditOperation.Delete, Now));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotAudit_WhenCoverDoesNotExist()
    {
        var deleted = await _service.DeleteAsync("unknown", _cancellationToken);

        Assert.False(deleted);
        await _auditQueue.DidNotReceiveWithAnyArgs().EnqueueAsync(default!);
    }

    private static CreateCoverRequest ValidRequest() => new(StartDate, StartDate.AddDays(30), CoverType.Yacht);
}

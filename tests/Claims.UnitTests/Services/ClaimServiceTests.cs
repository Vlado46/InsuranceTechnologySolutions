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

public class ClaimServiceTests
{
    private static readonly DateTime Now = new(2026, 7, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Cover Cover = new()
    {
        Id = "cover-1",
        StartDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
        EndDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private readonly IClaimRepository _claims = Substitute.For<IClaimRepository>();
    private readonly ICoverRepository _covers = Substitute.For<ICoverRepository>();
    private readonly IAuditQueue _auditQueue = Substitute.For<IAuditQueue>();
    private readonly ClaimService _service;

    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public ClaimServiceTests()
    {
        _covers.GetByIdAsync(Cover.Id, Arg.Any<CancellationToken>()).Returns(Cover);
        _service = new ClaimService(
            _claims,
            new CreateClaimRequestValidator(_covers),
            _auditQueue,
            new FakeTimeProvider(new DateTimeOffset(Now)));
    }

    [Fact]
    public async Task CreateAsync_StoresClaimWithGeneratedId()
    {
        var response = await _service.CreateAsync(ValidRequest(), _cancellationToken);

        Assert.True(Guid.TryParse(response.Id, out _));
        await _claims.Received(1).AddAsync(Arg.Is<Claim>(claim => claim.Id == response.Id), _cancellationToken);
    }

    [Fact]
    public async Task CreateAsync_ReadsDateWithoutOffsetAsUtc()
    {
        var created = new DateTime(2026, 7, 10);

        var response = await _service.CreateAsync(ValidRequest() with { Created = created }, _cancellationToken);

        Assert.Equal(DateTimeKind.Utc, response.Created.Kind);
        Assert.Equal(created, response.Created);
    }

    [Fact]
    public async Task CreateAsync_QueuesAuditEntry()
    {
        var response = await _service.CreateAsync(ValidRequest(), _cancellationToken);

        await _auditQueue.Received(1).EnqueueAsync(
            new AuditEntry(AuditTarget.Claim, response.Id, AuditOperation.Create, Now));
    }

    [Fact]
    public async Task CreateAsync_DoesNotStoreOrAudit_WhenRequestIsInvalid()
    {
        var request = ValidRequest() with { DamageCost = 100_001m };

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request, _cancellationToken));

        await _claims.DidNotReceiveWithAnyArgs().AddAsync(default!, _cancellationToken);
        await _auditQueue.DidNotReceiveWithAnyArgs().EnqueueAsync(default!);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenClaimDoesNotExist()
    {
        var response = await _service.GetByIdAsync("unknown", _cancellationToken);

        Assert.Null(response);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllClaims()
    {
        var claim = new Claim { Id = "claim-1", CoverId = Cover.Id, Name = "Hull damage", DamageCost = 5_000m };
        _claims.GetAllAsync(_cancellationToken).Returns([claim]);

        var response = await _service.GetAllAsync(_cancellationToken);

        Assert.Equal([ClaimResponse.From(claim)], response);
    }

    [Fact]
    public async Task DeleteAsync_QueuesAuditEntry_WhenClaimWasDeleted()
    {
        _claims.DeleteAsync("claim-1", _cancellationToken).Returns(true);

        var deleted = await _service.DeleteAsync("claim-1", _cancellationToken);

        Assert.True(deleted);
        await _auditQueue.Received(1).EnqueueAsync(
            new AuditEntry(AuditTarget.Claim, "claim-1", AuditOperation.Delete, Now));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotAudit_WhenClaimDoesNotExist()
    {
        var deleted = await _service.DeleteAsync("unknown", _cancellationToken);

        Assert.False(deleted);
        await _auditQueue.DidNotReceiveWithAnyArgs().EnqueueAsync(default!);
    }

    private static CreateClaimRequest ValidRequest() =>
        new(Cover.Id, Cover.StartDate.AddDays(5), "Hull damage", ClaimType.Collision, 50_000m);
}

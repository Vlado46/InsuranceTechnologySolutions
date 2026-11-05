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

public class ClaimServiceAuditTests
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

    public ClaimServiceAuditTests()
    {
        _covers.GetByIdAsync(Cover.Id, Arg.Any<CancellationToken>()).Returns(Cover);
        _service = new ClaimService(
            _claims,
            new CreateClaimRequestValidator(_covers),
            _auditQueue,
            new FakeTimeProvider(new DateTimeOffset(Now)));
    }

    [Fact]
    public async Task CreateAsync_QueuesAuditEntry()
    {
        var response = await _service.CreateAsync(ValidRequest(), _cancellationToken);

        await _auditQueue.Received(1).EnqueueAsync(
            new AuditEntry(AuditTarget.Claim, response.Id, AuditOperation.Create, Now));
    }

    [Fact]
    public async Task CreateAsync_DoesNotAudit_WhenRequestIsInvalid()
    {
        var request = ValidRequest() with { DamageCost = 100_001m };

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(request, _cancellationToken));

        await _auditQueue.DidNotReceiveWithAnyArgs().EnqueueAsync(default!);
    }

    [Fact]
    public async Task DeleteAsync_QueuesAuditEntry_WhenClaimWasDeleted()
    {
        _claims.DeleteAsync("claim-1", _cancellationToken).Returns(true);

        await _service.DeleteAsync("claim-1", _cancellationToken);

        await _auditQueue.Received(1).EnqueueAsync(
            new AuditEntry(AuditTarget.Claim, "claim-1", AuditOperation.Delete, Now));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotAudit_WhenClaimDoesNotExist()
    {
        await _service.DeleteAsync("unknown", _cancellationToken);

        await _auditQueue.DidNotReceiveWithAnyArgs().EnqueueAsync(default!);
    }

    private static CreateClaimRequest ValidRequest() =>
        new(Cover.Id, Cover.StartDate.AddDays(5), "Hull damage", ClaimType.Collision, 50_000m);
}

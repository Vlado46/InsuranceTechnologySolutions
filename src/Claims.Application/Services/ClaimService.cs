using Claims.Application.Contracts;
using Claims.Application.Interfaces;
using Claims.Domain;
using FluentValidation;

namespace Claims.Application.Services;

public class ClaimService(
    IClaimRepository claims,
    IValidator<CreateClaimRequest> validator,
    IAuditer auditer) : IClaimService
{
    public async Task<IReadOnlyList<ClaimResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var all = await claims.GetAllAsync(cancellationToken);
        return all.Select(ClaimResponse.From).ToList();
    }

    public async Task<ClaimResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var claim = await claims.GetByIdAsync(id, cancellationToken);
        return claim is null ? null : ClaimResponse.From(claim);
    }

    public async Task<ClaimResponse> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var claim = new Claim
        {
            Id = Guid.NewGuid().ToString(),
            CoverId = request.CoverId,
            Created = request.Created.AsUtc(),
            Name = request.Name,
            Type = request.Type,
            DamageCost = request.DamageCost
        };

        await claims.AddAsync(claim, cancellationToken);
        auditer.AuditClaim(claim.Id, "POST");

        return ClaimResponse.From(claim);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var deleted = await claims.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            auditer.AuditClaim(id, "DELETE");
        }

        return deleted;
    }
}

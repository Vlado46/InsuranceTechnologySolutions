using Claims.Application.Contracts;

namespace Claims.Application.Interfaces;

public interface IClaimService
{
    Task<IReadOnlyList<ClaimResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<ClaimResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<ClaimResponse> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken);

    /// <returns><c>false</c> when there is no claim with this id.</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}

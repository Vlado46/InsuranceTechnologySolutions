using Claims.Application.Contracts;

namespace Claims.Application.Interfaces;

public interface ICoverService
{
    Task<IReadOnlyList<CoverResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<CoverResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<CoverResponse> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken);

    /// <returns><c>false</c> when there is no cover with this id.</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);

    Task<decimal> ComputePremiumAsync(CreateCoverRequest request, CancellationToken cancellationToken);
}

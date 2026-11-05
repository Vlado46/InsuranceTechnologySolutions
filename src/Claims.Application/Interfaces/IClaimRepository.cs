using Claims.Domain;

namespace Claims.Application.Interfaces;

public interface IClaimRepository
{
    Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken cancellationToken);

    Task<Claim?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task AddAsync(Claim claim, CancellationToken cancellationToken);

    /// <returns><c>false</c> when there is no claim with this id.</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}

using Claims.Application.Contracts;
using Claims.Domain;

namespace Claims.Application.Interfaces;

public interface ICoverService
{
    Task<IReadOnlyList<CoverResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<CoverResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<CoverResponse> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(string id, CancellationToken cancellationToken);

    decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType);
}

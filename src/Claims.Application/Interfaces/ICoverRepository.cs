using Claims.Domain;

namespace Claims.Application.Interfaces;

public interface ICoverRepository
{
    Task<IReadOnlyList<Cover>> GetAllAsync(CancellationToken cancellationToken);

    Task<Cover?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task AddAsync(Cover cover, CancellationToken cancellationToken);

    /// <returns><c>false</c> when there is no cover with this id.</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
}

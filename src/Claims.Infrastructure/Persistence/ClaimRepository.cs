using Claims.Application.Interfaces;
using Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence;

public class ClaimRepository(ClaimsContext context) : IClaimRepository
{
    public async Task<IReadOnlyList<Claim>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Claims.ToListAsync(cancellationToken);

    public Task<Claim?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        context.Claims.SingleOrDefaultAsync(claim => claim.Id == id, cancellationToken);

    public async Task AddAsync(Claim claim, CancellationToken cancellationToken)
    {
        context.Claims.Add(claim);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var claim = await GetByIdAsync(id, cancellationToken);
        if (claim is null)
        {
            return false;
        }

        context.Claims.Remove(claim);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

using Claims.Application.Contracts;
using Claims.Application.Interfaces;
using Claims.Domain;

namespace Claims.Application.Services;

public class CoverService(ICoverRepository covers, IAuditer auditer) : ICoverService
{
    public async Task<IReadOnlyList<CoverResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var all = await covers.GetAllAsync(cancellationToken);
        return all.Select(CoverResponse.From).ToList();
    }

    public async Task<CoverResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var cover = await covers.GetByIdAsync(id, cancellationToken);
        return cover is null ? null : CoverResponse.From(cover);
    }

    public async Task<CoverResponse> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken)
    {
        var cover = new Cover
        {
            Id = Guid.NewGuid().ToString(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Type = request.Type,
            Premium = PremiumCalculator.Compute(request.StartDate, request.EndDate, request.Type)
        };

        await covers.AddAsync(cover, cancellationToken);
        auditer.AuditCover(cover.Id, "POST");

        return CoverResponse.From(cover);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var deleted = await covers.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            auditer.AuditCover(id, "DELETE");
        }

        return deleted;
    }

    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType) =>
        PremiumCalculator.Compute(startDate, endDate, coverType);
}

using Claims.Application.Contracts;
using Claims.Application.Interfaces;
using Claims.Domain;
using FluentValidation;

namespace Claims.Application.Services;

public class CoverService(
    ICoverRepository covers,
    IValidator<CreateCoverRequest> validator,
    IAuditer auditer) : ICoverService
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
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var startDate = request.StartDate.AsUtc();
        var endDate = request.EndDate.AsUtc();
        var cover = new Cover
        {
            Id = Guid.NewGuid().ToString(),
            StartDate = startDate,
            EndDate = endDate,
            Type = request.Type,
            Premium = PremiumCalculator.Compute(startDate, endDate, request.Type)
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

    public async Task<decimal> ComputePremiumAsync(CreateCoverRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        return PremiumCalculator.Compute(request.StartDate.AsUtc(), request.EndDate.AsUtc(), request.Type);
    }
}

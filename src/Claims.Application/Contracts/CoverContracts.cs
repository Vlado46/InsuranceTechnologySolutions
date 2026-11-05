using Claims.Domain;

namespace Claims.Application.Contracts;

public record CreateCoverRequest(
    DateTime StartDate,
    DateTime EndDate,
    CoverType Type);

public record CoverResponse(
    string Id,
    DateTime StartDate,
    DateTime EndDate,
    CoverType Type,
    decimal Premium)
{
    public static CoverResponse From(Cover cover) =>
        new(cover.Id, cover.StartDate, cover.EndDate, cover.Type, cover.Premium);
}

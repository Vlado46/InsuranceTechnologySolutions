using Claims.Application.Contracts;
using Claims.Application.Interfaces;
using Claims.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController(ICoverService covers) : ControllerBase
{
    [HttpPost("compute")]
    public async Task<ActionResult<decimal>> ComputePremiumAsync(
        DateTime startDate, DateTime endDate, CoverType coverType, CancellationToken cancellationToken) =>
        Ok(await covers.ComputePremiumAsync(new CreateCoverRequest(startDate, endDate, coverType), cancellationToken));

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoverResponse>>> GetAsync(CancellationToken cancellationToken) =>
        Ok(await covers.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<ActionResult<CoverResponse>> GetAsync(string id, CancellationToken cancellationToken)
    {
        var cover = await covers.GetByIdAsync(id, cancellationToken);
        return cover is null ? NotFound() : Ok(cover);
    }

    [HttpPost]
    public async Task<ActionResult<CoverResponse>> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken) =>
        Ok(await covers.CreateAsync(request, cancellationToken));

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(string id, CancellationToken cancellationToken) =>
        await covers.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}

using Claims.Application.Contracts;
using Claims.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ClaimsController(IClaimService claims) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<ClaimResponse>> GetAsync(CancellationToken cancellationToken) =>
        await claims.GetAllAsync(cancellationToken);

    [HttpPost]
    public async Task<ActionResult<ClaimResponse>> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken) =>
        Ok(await claims.CreateAsync(request, cancellationToken));

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(string id, CancellationToken cancellationToken) =>
        await claims.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpGet("{id}")]
    public async Task<ActionResult<ClaimResponse>> GetAsync(string id, CancellationToken cancellationToken)
    {
        var claim = await claims.GetByIdAsync(id, cancellationToken);
        return claim is null ? NotFound() : Ok(claim);
    }
}

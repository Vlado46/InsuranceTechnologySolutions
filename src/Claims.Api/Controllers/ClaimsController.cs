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
    public Task DeleteAsync(string id, CancellationToken cancellationToken) =>
        claims.DeleteAsync(id, cancellationToken);

    [HttpGet("{id}")]
    public Task<ClaimResponse?> GetAsync(string id, CancellationToken cancellationToken) =>
        claims.GetByIdAsync(id, cancellationToken);
}

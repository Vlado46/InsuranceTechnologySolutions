using Claims.Application.Contracts;
using Claims.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

/// <summary>Maintains the claims reported against covers.</summary>
[ApiController]
[Route("[controller]")]
public class ClaimsController(IClaimService claims) : ControllerBase
{
    /// <summary>Returns all claims.</summary>
    [HttpGet]
    public async Task<IEnumerable<ClaimResponse>> GetAsync(CancellationToken cancellationToken) =>
        await claims.GetAllAsync(cancellationToken);

    /// <summary>Creates a claim. The damage cannot exceed 100.000 and the date must be within the period of the cover.</summary>
    [HttpPost]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClaimResponse>> CreateAsync(CreateClaimRequest request, CancellationToken cancellationToken) =>
        Ok(await claims.CreateAsync(request, cancellationToken));

    /// <summary>Deletes a claim.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(string id, CancellationToken cancellationToken) =>
        await claims.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    /// <summary>Returns one claim.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimResponse>> GetAsync(string id, CancellationToken cancellationToken)
    {
        var claim = await claims.GetByIdAsync(id, cancellationToken);
        return claim is null ? NotFound() : Ok(claim);
    }
}

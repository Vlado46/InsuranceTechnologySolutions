using Claims.Application.Contracts;
using Claims.Application.Interfaces;
using Claims.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

/// <summary>Maintains the insurance covers and quotes their premium.</summary>
[ApiController]
[Route("[controller]")]
public class CoversController(ICoverService covers) : ControllerBase
{
    /// <summary>Computes the premium for a period without creating a cover.</summary>
    [HttpPost("compute")]
    [ProducesResponseType<decimal>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<decimal>> ComputePremiumAsync(
        DateTime startDate, DateTime endDate, CoverType coverType, CancellationToken cancellationToken) =>
        Ok(await covers.ComputePremiumAsync(new CreateCoverRequest(startDate, endDate, coverType), cancellationToken));

    /// <summary>Returns all covers.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoverResponse>>> GetAsync(CancellationToken cancellationToken) =>
        Ok(await covers.GetAllAsync(cancellationToken));

    /// <summary>Returns one cover.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType<CoverResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoverResponse>> GetAsync(string id, CancellationToken cancellationToken)
    {
        var cover = await covers.GetByIdAsync(id, cancellationToken);
        return cover is null ? NotFound() : Ok(cover);
    }

    /// <summary>Creates a cover. It cannot start in the past or last longer than 1 year.</summary>
    [HttpPost]
    [ProducesResponseType<CoverResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CoverResponse>> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken) =>
        Ok(await covers.CreateAsync(request, cancellationToken));

    /// <summary>Deletes a cover. Its claims are not touched.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(string id, CancellationToken cancellationToken) =>
        await covers.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}

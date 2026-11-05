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
    public ActionResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType) =>
        Ok(covers.ComputePremium(startDate, endDate, coverType));

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoverResponse>>> GetAsync(CancellationToken cancellationToken) =>
        Ok(await covers.GetAllAsync(cancellationToken));

    [HttpGet("{id}")]
    public async Task<ActionResult<CoverResponse>> GetAsync(string id, CancellationToken cancellationToken) =>
        Ok(await covers.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CoverResponse>> CreateAsync(CreateCoverRequest request, CancellationToken cancellationToken) =>
        Ok(await covers.CreateAsync(request, cancellationToken));

    [HttpDelete("{id}")]
    public Task DeleteAsync(string id, CancellationToken cancellationToken) =>
        covers.DeleteAsync(id, cancellationToken);
}

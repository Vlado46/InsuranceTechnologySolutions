using Claims.Application.Interfaces;
using Claims.Domain;
using Claims.Infrastructure.Auditing;
using Microsoft.AspNetCore.Mvc;


namespace Claims.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly ILogger<ClaimsController> _logger;
        private readonly IClaimRepository _claims;
        private readonly Auditer _auditer;

        public ClaimsController(ILogger<ClaimsController> logger, IClaimRepository claims, AuditContext auditContext)
        {
            _logger = logger;
            _claims = claims;
            _auditer = new Auditer(auditContext);
        }

        [HttpGet]
        public async Task<IEnumerable<Claim>> GetAsync(CancellationToken cancellationToken)
        {
            return await _claims.GetAllAsync(cancellationToken);
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(Claim claim, CancellationToken cancellationToken)
        {
            claim.Id = Guid.NewGuid().ToString();
            await _claims.AddAsync(claim, cancellationToken);
            _auditer.AuditClaim(claim.Id, "POST");
            return Ok(claim);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            _auditer.AuditClaim(id, "DELETE");
            await _claims.DeleteAsync(id, cancellationToken);
        }

        [HttpGet("{id}")]
        public async Task<Claim?> GetAsync(string id, CancellationToken cancellationToken)
        {
            return await _claims.GetByIdAsync(id, cancellationToken);
        }
    }
}

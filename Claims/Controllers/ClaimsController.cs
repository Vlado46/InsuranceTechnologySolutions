using Claims.Auditing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;


namespace Claims.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly ILogger<ClaimsController> _logger;
        private readonly ClaimsContext _claimsContext;
        private readonly Auditer _auditer;

        public ClaimsController(ILogger<ClaimsController> logger, ClaimsContext claimsContext, AuditContext auditContext)
        {
            _logger = logger;
            _claimsContext = claimsContext;
            _auditer = new Auditer(auditContext);
        }

        [HttpGet]
        public async Task<IEnumerable<Claim>> GetAsync()
        {
            return await _claimsContext.GetClaimsAsync();
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(Claim claim)
        {
            claim.Id = Guid.NewGuid().ToString();
            await _claimsContext.AddItemAsync(claim);
            _auditer.AuditClaim(claim.Id, "POST");
            return Ok(claim);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(string id)
        {
            _auditer.AuditClaim(id, "DELETE");
            await _claimsContext.DeleteItemAsync(id);
        }

        [HttpGet("{id}")]
        public async Task<Claim> GetAsync(string id)
        {
            return await _claimsContext.GetClaimAsync(id);
        }
    }

    public class ClaimsContext : DbContext
    {

        private DbSet<Claim> Claims { get; init; }
        public DbSet<Cover>  Covers { get; init; }

        public ClaimsContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Claim>(claim =>
            {
                claim.ToCollection("claims");
                claim.Property(c => c.CoverId).HasElementName("coverId");
                claim.Property(c => c.Created).HasElementName("created");
                claim.Property(c => c.Name).HasElementName("name");
                claim.Property(c => c.Type).HasElementName("claimType");
                claim.Property(c => c.DamageCost).HasElementName("damageCost");
            });

            modelBuilder.Entity<Cover>(cover =>
            {
                cover.ToCollection("covers");
                cover.Property(c => c.StartDate).HasElementName("startDate");
                cover.Property(c => c.EndDate).HasElementName("endDate");
                cover.Property(c => c.Type).HasElementName("claimType");
                cover.Property(c => c.Premium).HasElementName("premium");
            });
        }

        public async Task<IEnumerable<Claim>> GetClaimsAsync()
        {
            return await Claims.ToListAsync();
        }

        public async Task<Claim> GetClaimAsync(string id)
        {
            return await Claims
                .Where(claim => claim.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task AddItemAsync(Claim item)
        {
            Claims.Add(item);
            await SaveChangesAsync();
        }

        public async Task DeleteItemAsync(string id)
        {
            var claim = await GetClaimAsync(id);
            if (claim is not null)
            {
                Claims.Remove(claim);
                await SaveChangesAsync();
            }
        }
    }
}

using Claims.Domain;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Claims.Infrastructure.Persistence
{
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

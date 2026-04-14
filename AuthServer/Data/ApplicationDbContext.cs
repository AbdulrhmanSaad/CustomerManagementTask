using AuthServer.Domain;
using AuthServer.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace AuthServer.Data
{
    public class ApplicationDbContext: IdentityDbContext<User>
    {
        private readonly ITenantService tenantService;
        public string? TenantId { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,ITenantService _tenantService) : base(options)
        {
            tenantService = _tenantService;
            TenantId = tenantService?.GetCurrentTenant()?.TenantId;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasQueryFilter(u => string.IsNullOrEmpty(TenantId) || u.TenantId == TenantId);

            builder.UseOpenIddict();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                entry.Entity.TenantId = TenantId;
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}

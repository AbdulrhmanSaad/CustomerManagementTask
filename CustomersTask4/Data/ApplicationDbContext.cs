using CustomersTask4.Domain;
using CustomersTask4.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace CustomersTask4.Data
{
    [ExcludeFromCodeCoverage]
    public class ApplicationDbContext:IdentityDbContext<User>
    {
        private readonly ITenantService tenantService;
        public string? TenantId { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,ITenantService tenantService)
            : base(options)
        {
            this.tenantService = tenantService;
            TenantId = tenantService?.GetCurrentTenant()?.TenantId;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Phone)
                .IsUnique();

            modelBuilder.Entity<Address>()
                .HasIndex(a=> new {a.CustomerId,a.AddressType})
                .IsUnique();



            modelBuilder.Entity<Customer>()
                 .HasMany(c => c.Addresses)
                 .WithOne(a => a.Customer)
                 .HasForeignKey(a => a.CustomerId)
                 .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .ToTable("Customers", b => b.IsTemporal());

            modelBuilder.Entity<Address>()
                .ToTable("Addresses", b => b.IsTemporal());

            // Global Query Filter for multi-tenancy
            modelBuilder.Entity<Customer>()
                .HasQueryFilter(c => c.TenantId == TenantId && !c.IsDeleted);

            modelBuilder.Entity<User>()
              .HasQueryFilter(u => u.TenantId == TenantId);

            modelBuilder.Entity<Address>()
              .HasQueryFilter(a => a.TenantId == TenantId);


            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Phone)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        }
        
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Domain.WebhookSubscription> WebhookSubscriptions { get; set; }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>()
                .Where(e=>e.State==EntityState.Added||e.State==EntityState.Modified))
            {
               entry.Entity.TenantId = TenantId;
            }
            return base.SaveChangesAsync(cancellationToken);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var tenantConnectionString = tenantService.GetConnectionString();
         

            if (!string.IsNullOrEmpty(tenantConnectionString))

            {
                var provider = tenantService.GetDatabaseProvider()?.ToLower();

                if (provider == "sql")
                {
                    optionsBuilder.UseSqlServer(tenantConnectionString);
                }
            }
        }

    }
    
}

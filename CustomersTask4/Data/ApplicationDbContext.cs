using Castle.Components.DictionaryAdapter.Xml;
using Castle.Core.Resource;
using CustomersTask4.Domain;
using CustomersTask4.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.AccessControl;
using System.Text.Json;

namespace CustomersTask4.Data
{
    public class ApplicationDbContext:IdentityDbContext<User>
    {


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
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
              .HasMany(c => c.History)
              .WithOne(c=>c.Customer)
              .HasForeignKey(a => a.CustomerId)
              .OnDelete(DeleteBehavior.NoAction);

        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<CustomerHistory> CustomerHistories { get; set; }
        //audit on table Customer and Address

        //public override int SaveChanges()
        //{
        //    AuditEntities();
        //    return base.SaveChanges();
        //}

        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    AuditEntities();
        //    return await base.SaveChangesAsync(cancellationToken);
        //}

        //private void AuditEntities()
        //{
        //       var currentUser = user.GetCurrentUser();
        //    var entries = ChangeTracker.Entries<Customer>()
        //        .Where(e => e.State == EntityState.Modified
        //                 || e.State == EntityState.Deleted
        //                 || e.State == EntityState.Added).ToList();

        //    foreach (var entry in entries)
        //    {
        //        var customer = entry.Entity;

        //        var audit = new CustomerHistory
        //        {
        //            CustomerId = entry.Entity.Id,
        //            Action = entry.State.ToString(),
        //            ChangedAt = DateTime.Now,
        //            UserEmail = currentUser.Name,
        //            UserId = currentUser.Id,
        //            UserRole = currentUser.Roles.FirstOrDefault() ?? "No Role",
        //            OldValues = entry.State == EntityState.Modified || entry.State == EntityState.Deleted
        //                      ? JsonSerializer.Serialize(entry.OriginalValues.ToObject())
        //                      : "{}",
        //            NewValues = entry.State == EntityState.Modified || entry.State == EntityState.Added
        //                      ? JsonSerializer.Serialize(new 
        //                      {
        //                           customer.Id,
        //                            customer.Name,
        //                                customer.Phone,
        //                            customer.CreatedAt,
        //                                    customer.CreatedBy,
        //                             Addresses = customer.Addresses.ToList()
        //                       })
        //                      : "{}"
        //        };

        //        CustomerHistories.Add(audit);
        //    }
        //}









        //public override async Task<int> SaveChangesAsync(
        //           bool acceptAllChangesOnSuccess,
        //           CancellationToken cancellationToken = default)
        //{
        //    var auditEntries = new List<CustomerHistory>();
        //    var currentUser = user.GetCurrentUser();

        //    // Collect audit data for existing changes
        //    foreach (var entry in ChangeTracker.Entries<Customer>().ToList())
        //    {
        //        if (entry.State != EntityState.Modified || entry.State == EntityState.Added)
        //            continue;

        //        var audit = new CustomerHistory
        //        {
        //            CustomerId = entry.Entity.Id,
        //            ChangedAt = DateTime.UtcNow,
        //            Action = entry.State.ToString(),
        //            UserId = currentUser.Id,
        //            UserEmail = currentUser.Name,
        //            UserRole = currentUser.Roles.FirstOrDefault() ?? "No Role"
        //        };

        //        // Track only scalar properties to avoid recursion
        //        var oldValues = new Dictionary<string, object>();
        //        var newValues = new Dictionary<string, object>();

        //        foreach (var property in entry.Properties)
        //        {
        //            // Skip navigation properties (lists or complex objects)
        //            if (property.Metadata.ClrType.IsClass && property.Metadata.ClrType != typeof(string))
        //                continue;

        //            if (entry.State == EntityState.Modified && property.IsModified)
        //            {
        //                oldValues[property.Metadata.Name] = property.OriginalValue!;
        //                newValues[property.Metadata.Name] = property.CurrentValue!;
        //            }
        //            else if (entry.State == EntityState.Added)
        //            {
        //                newValues[property.Metadata.Name] = property.CurrentValue!;
        //            }
        //        }

        //        audit.OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : "{}";
        //        audit.NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : "{}";
        //        auditEntries.Add(audit);
        //    }

        //    // Save all Customer changes FIRST (this generates IDs and updates state)
        //    var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        //    // Now create audit entries AFTER save (CustomerId is now valid)
        //    if (auditEntries.Count > 0)
        //    {
        //        CustomerHistories.AddRange(auditEntries);
        //        await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        //    }

        //    return result;
        //}

    }
}

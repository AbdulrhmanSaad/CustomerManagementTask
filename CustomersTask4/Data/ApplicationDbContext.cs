using Castle.Components.DictionaryAdapter.Xml;
using Castle.Core.Resource;
using CustomersTask4.Domain;
using CustomersTask4.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
using System.Text.Json;

namespace CustomersTask4.Data
{
    [ExcludeFromCodeCoverage]
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
                 .HasMany(c => c.Addresses)
                 .WithOne(a => a.Customer)
                 .HasForeignKey(a => a.CustomerId)
                 .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Customer>()
                .ToTable("Customers", b => b.IsTemporal());

            modelBuilder.Entity<Address>()
                .ToTable("Addresses", b => b.IsTemporal());

        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        

    }
}

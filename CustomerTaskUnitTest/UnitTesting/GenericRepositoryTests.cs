using CustomersTask4.Data;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using CustomersTask4.Domain;
using CustomersTask4.Setting;
using NSubstitute;
using Microsoft.Extensions.Options;

namespace CustomerTaskUnitTest.UnitTesting
{
    public class GenericRepositoryTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Create a mock ITenantService
            var tenantService = Substitute.For<ITenantService>();
            
            // Mock the required methods
            tenantService.GetConnectionString().Returns((string)null);
            tenantService.GetDatabaseProvider().Returns("sql");
            var tenant = new Tenant
            {
                TenantId = "Tenant1"
            };
            tenantService.GetCurrentTenant().Returns(tenant);

            return new ApplicationDbContext(options, tenantService);
        }

        [Fact]
        public async Task Add_ShouldAddEntity()
        {
            var context = GetDbContext();
            var repo = new GenericRepository<Customer>(context);

            var entity = new Customer { Name = "Test", Phone = "123", CreatedBy = "abdo@gmail.com" };

            await repo.Add(entity);

            Assert.Equal(1, context.Set<Customer>().Count());
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntity()
        {
            var context = GetDbContext();
            var repo = new GenericRepository<Customer>(context);

            var entity = new Customer { Name = "Test", Phone = "123", CreatedBy = "abdo@gmail.com" };
            context.Add(entity);
            await context.SaveChangesAsync();

            await repo.Delete(entity);

            Assert.Empty(context.Set<Customer>());
        }

        [Fact]
        public void GetAll_ShouldReturnAllEntities()
        {
            var context = GetDbContext();

            var repo = new GenericRepository<Customer>(context);

            context.AddRange(new List<Customer>
        {
            new Customer { Name = "Test", Phone = "123", CreatedBy="abdo@gmail.com" },
            new Customer { Name = "Test", Phone = "123", CreatedBy="abdo@gmail.com" }
        });
            context.SaveChangesAsync();

            var result = repo.GetAll();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
        {
            var context = GetDbContext();
            var repo = new GenericRepository<Customer>(context);

            var entity = new Customer { Name = "Test", Phone = "123", CreatedBy = "abdo@gmail.com" };
            context.Add(entity);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync(entity.Id);

            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var context = GetDbContext();
            var repo = new GenericRepository<Customer>(context);

            var result = await repo.GetByIdAsync("999");

            Assert.Null(result);
        }

        [Fact]
        public async Task Update_ShouldModifyEntity()
        {
            var context = GetDbContext();
            var repo = new GenericRepository<Customer>(context);

            var entity = new Customer { Name = "Test", Phone = "123", CreatedBy = "abdo@gmail.com" };
            context.Add(entity);
            await context.SaveChangesAsync();

            entity.Name = "Updated";

            await repo.Update(entity);

            var updated = context.Set<Customer>().First();
            Assert.Equal("Updated", updated.Name);
        }

        [Fact]
        public void PhoneExistsAsync_ShouldReturnTrue_WhenPhoneExists()
        {
            var context = GetDbContext();
            var repo = new GenericRepository<Customer>(context);
            var customer = new Customer { Name = "Test", Phone = "123", CreatedBy = "abdo@gmail.com" };

            context.Add(new Customer { Name = "Test", Phone = "123" , CreatedBy = "abdo@gmail.com" });
            context.SaveChangesAsync();

            var result = repo.PhoneExistsAsync("123");

            Assert.True(result);
        }

        [Fact]
        public void PhoneExistsAsync_ShouldReturnFalse_WhenPhoneNotExists()
        {
            var context = GetDbContext();
            var repo = new GenericRepository<Customer>(context);

            var result = repo.PhoneExistsAsync("01055559742");

            Assert.False(result);
        }
    }
}
using Azure.Core;
using Castle.Core.Logging;
using CustomersTask4.Abstraction;
using CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand;
using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.Data;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Hubs;
using CustomersTask4.Repository;
using CustomersTask4.Services;
using CustomersTask4.Services.Caching;
using CustomersTask4.Setting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CustomerTaskUnitTest.UnitTesting
{
    public class DeleteCustomerCommandHandlerTest
    {
        private readonly IGenericRepository<Customer> repository; 
        private readonly ILogger<DeleteCustomerCommandHandler> logger;
        private readonly DeleteCustomerCommandHandler _handler;
        private readonly ApplicationDbContext db;
        private readonly IConfiguration configuration;
        private readonly IHubContext<MessageHub> hubContext;
        private readonly IAppMeditor bus;
        private readonly ILocalizationService localization;
        private readonly HybridCache cachingService;



        public DeleteCustomerCommandHandlerTest()
        {
            repository = Substitute.For<IGenericRepository<Customer>>();
            configuration = Substitute.For<IConfiguration>();
            hubContext = Substitute.For<IHubContext<MessageHub>>();
            bus = Substitute.For<IAppMeditor>();
            localization = Substitute.For<ILocalizationService>();
            cachingService = Substitute.For<HybridCache>();
            logger = Substitute.For<ILogger<DeleteCustomerCommandHandler>>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
               .Options;

            var tenantService = Substitute.For<ITenantService>();
            tenantService.GetCurrentTenant().Returns(new Tenant
            {
                TenantId = "Tenant1",
                Name = "Tenant",
                ConnectionString = ""
            });

            db = new ApplicationDbContext(options, tenantService);

            _handler = new DeleteCustomerCommandHandler(repository,db, logger,configuration,hubContext,
                 bus,localization,cachingService);
        }
        [Fact]
        public async Task Handler_ShouldDeleteCustomerSuccessfully()
        {
            // Arrange
            var command = new DeleteCustomerCommand("32");
            var existingCustomer = new Customer
            {
                Id = "32",
                Name = "Ahmed",
                Phone = "01013513652",
                CreatedAt = DateTime.Now,
                CreatedBy = "admin"
            };
            repository.GetByIdAsync(command.Id).Returns(existingCustomer);
            existingCustomer.IsDeleted = true;
            db.SaveChanges();

            //Act
             await _handler.Handle(command,CancellationToken.None);

            //assert
            await repository.Received(1).GetByIdAsync(command.Id);

        }

        [Fact]
        public async Task Handle_WithIdNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var command = new DeleteCustomerCommand("1");
            var existingCustomer = new Customer
            {
                Id = "1",
                Name = "Ahmed",
                Phone = "01013513652",
                CreatedAt = DateTime.Now,
                CreatedBy = "admin"
            };

            repository.GetByIdAsync(command.Id, Arg.Any<Expression<Func<Customer, object>>>())
                .Returns(existingCustomer);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            Assert.Equal(localization.Localize("Customer With Id={0} not found",command.Id), exception.Message);
            await repository.DidNotReceive().Delete(Arg.Any<Customer>());
        }

    }
}

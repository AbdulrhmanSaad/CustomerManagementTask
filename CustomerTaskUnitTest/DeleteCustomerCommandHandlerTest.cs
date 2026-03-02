using Azure.Core;
using Castle.Core.Logging;
using CustomersTask4.CustomerHandler.Command.DeleteCustomerCommand;
using CustomersTask4.CustomerHandler.Command.UpdateCustomer;
using CustomersTask4.Domain;
using CustomersTask4.Exceptions;
using CustomersTask4.Repository;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerTaskUnitTest
{
    public class DeleteCustomerCommandHandlerTest
    {
        private readonly IGenericRepository<Customer> repository; 
        private readonly ILogger<DeleteCustomerCommandHandler> logger;
        private readonly DeleteCustomerCommandHandler _handler;


        public DeleteCustomerCommandHandlerTest()
        {
            this.repository = Substitute.For<IGenericRepository<Customer>>();
            this.logger = Substitute.For<ILogger<DeleteCustomerCommandHandler>>();
             _handler = new DeleteCustomerCommandHandler(repository, logger);
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
            repository.Delete(existingCustomer).Returns(Task.CompletedTask);

            //Act
             await _handler.Handle(command,CancellationToken.None);

            //assert
            await repository.Received(1).Delete(Arg.Any<Customer>());

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

            repository.GetByIdAsync(command.Id, Arg.Any<System.Linq.Expressions.Expression<System.Func<Customer, object>>>())
                .Returns(existingCustomer);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _handler.Handle(command, CancellationToken.None).AsTask()
            );

            Assert.Equal($"Customer With Id={command.Id} not found", exception.Message);
            await repository.DidNotReceive().Delete(Arg.Any<Customer>());
        }

    }
}
